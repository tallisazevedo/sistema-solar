using System.Text.Json;
using Hangfire;
using SolarES.Aplicacao.Configuracao;
using SolarES.Aplicacao.Metricas;
using SolarES.Aplicacao.Propostas;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Lead;
using SolarES.Dominio.Proposta;
using SolarES.Dominio.Simulacao;

namespace SolarES.Aplicacao.Leads;

public sealed class LeadAppService(ILeadRepository leads, ISimulacaoRepository simulacoes,
    IConfiguracaoVersaoRepository configuracoes, PropostaAppService propostas,
    EnvioPropostaAppService envios, IBackgroundJobClient jobs,
    IArmazenamentoAnexoConta armazenamentoAnexos, ConfiguracaoConsentimentos configuracaoConsentimentos,
    ConfiguracaoLimiteEnvios limiteEnvios, FunilAppService funil,
    ConfiguracaoRetencaoLgpd configuracaoRetencao, TimeProvider relogio)
{
    public async Task<DesfechoCapturaLead> CapturarPublicoAsync(Guid simulacaoId, string nome,
        string telefone, string email, CanalPreferido canal,
        IReadOnlyCollection<FinalidadeConsentimento> finalidadesAceitas, string versaoTexto,
        byte[]? conteudoAnexo, Guid? sessaoFunilId, CancellationToken ct)
    {
        if (!configuracaoConsentimentos.VersoesTextoAceitas.Contains(versaoTexto))
            throw new ArgumentException("Versão do texto de consentimento desconhecida.");
        if (conteudoAnexo is not null && !finalidadesAceitas.Contains(FinalidadeConsentimento.GuardaAnexoConta))
            throw new ArgumentException("O consentimento para guarda do anexo é obrigatório para anexar a conta.");
        TipoAnexoConta? tipoAnexo = conteudoAnexo is null ? null : IdentificarTipoAnexo(conteudoAnexo);
        var simulacao = await simulacoes.ObterPorIdAsync(simulacaoId, ct);
        if (simulacao is null || simulacao.Origem != OrigemSimulacao.Landing)
            throw new KeyNotFoundException("Simulação pública não encontrada.");
        var entrada = JsonSerializer.Deserialize<EntradaSimulacao>(simulacao.EntradasSnapshot)
            ?? throw new InvalidOperationException("Entradas da simulação inválidas.");
        var municipioId = await leads.ObterMunicipioIdPorCodigoAsync(entrada.MunicipioCodigoIbge, ct)
            ?? throw new InvalidOperationException("Município da simulação não encontrado.");
        var agora = relogio.GetUtcNow();

        // Contagem ANTES de persistir este lead -- senao ele contaria contra o proprio
        // limite (issue #33).
        var emailNormalizado = email.Trim().ToLowerInvariant();
        var telefoneNormalizado = telefone.Trim();
        var leadsRecentes = await leads.ContarPorContatoDesdeAsync(
            emailNormalizado, telefoneNormalizado, agora - limiteEnvios.Janela, ct);

        var lead = Lead.Criar(nome, telefone, email, canal, simulacao.Id, municipioId,
            finalidadesAceitas, agora);
        leads.Adicionar(lead);
        foreach (var finalidade in finalidadesAceitas.Distinct())
            leads.AdicionarConsentimento(ConsentimentoLgpd.Criar(lead.Id, finalidade, versaoTexto, agora));
        simulacao.LeadId = lead.Id;
        if (conteudoAnexo is not null && tipoAnexo is { } tipo)
        {
            var caminho = await armazenamentoAnexos.SalvarAsync(lead.Id, tipo, conteudoAnexo, ct);
            var recebidoEm = relogio.GetUtcNow();
            leads.AdicionarAnexo(AnexoConta.Criar(lead.Id, tipo, conteudoAnexo.LongLength,
                caminho, recebidoEm, recebidoEm.AddDays(configuracaoRetencao.PrazoDescarteAnexoDias)));
        }
        await leads.SalvarAlteracoesAsync(ct);
        if (sessaoFunilId is { } sessao)
        {
            await funil.RegistrarLeadCapturadoAsync(sessao, ct);
            if (conteudoAnexo is not null) await funil.RegistrarAnexoOferecidoAsync(sessao, ct);
        }

        var versao = await configuracoes.ObterPorIdAsync(simulacao.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versão da configuração não encontrada.");
        if (simulacao.RoteadaParaHumano) return DesfechoCapturaLead.RoteadoParaHumano;
        if (versao.Payload.PossuiPremissaProvisoria()) return DesfechoCapturaLead.CalibracaoPendente;

        // Anti-spam (issue #33): o lead e' registrado normalmente, so' o envio
        // automatico e' suprimido -- resposta identica a calibracao pendente, pra nao
        // revelar ao visitante que o limite existe.
        if (leadsRecentes >= limiteEnvios.LimitePorDestino) return DesfechoCapturaLead.CalibracaoPendente;

        var (proposta, jobIdGeracaoPdf) = await propostas.GerarComJobIdAsync(simulacao.Id, null, ct);

        // Envio automatico pelo canal escolhido na landing -- mesma guarda de
        // calibracao do envio manual do admin, mas o disparo em si e' uma
        // continuacao do job de geracao do PDF (so' roda depois que o PDF existir).
        var canalEnvio = canal == CanalPreferido.Email ? CanalEnvio.Email : CanalEnvio.Whatsapp;
        var destino = canal == CanalPreferido.Email ? email : telefone;
        var envio = await envios.PrepararEnvioAsync(proposta.Id, canalEnvio, destino, ct);
        jobs.ContinueJobWith<EnviarPropostaJob>(jobIdGeracaoPdf, job => job.ExecutarAsync(envio.Id, CancellationToken.None));

        return DesfechoCapturaLead.PropostaEmitida;
    }

    /// <summary>Eliminacao imediata a pedido do titular -- mesma anonimizacao do expurgo automatico.</summary>
    public async Task<bool> EliminarAsync(Guid leadId, CancellationToken ct)
    {
        var lead = await leads.ObterPorIdAsync(leadId, ct);
        if (lead is null) return false;
        var anexo = await leads.ObterAnexoAsync(leadId, ct);
        await AnonimizacaoLead.ExecutarAsync(lead, anexo, armazenamentoAnexos, relogio.GetUtcNow(), ct);
        await leads.SalvarAlteracoesAsync(ct);
        return true;
    }

    private static TipoAnexoConta IdentificarTipoAnexo(byte[] conteudo)
    {
        if (conteudo.AsSpan().StartsWith("%PDF-"u8)) return TipoAnexoConta.Pdf;
        if (conteudo.AsSpan().StartsWith(new byte[] { 0xFF, 0xD8, 0xFF })) return TipoAnexoConta.Jpeg;
        if (conteudo.AsSpan().StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return TipoAnexoConta.Png;
        throw new ArgumentException("O anexo deve ser um arquivo PDF, JPEG ou PNG válido.");
    }
}

public enum DesfechoCapturaLead { CalibracaoPendente, RoteadoParaHumano, PropostaEmitida }
public sealed record ConfiguracaoConsentimentos(IReadOnlySet<string> VersoesTextoAceitas);
public sealed record ConfiguracaoRetencaoLgpd(int PrazoDescarteAnexoDias, int PrazoExpurgoLeadMeses);
