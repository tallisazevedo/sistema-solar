using System.Text.Json;
using SolarES.Aplicacao.Configuracao;
using SolarES.Aplicacao.Propostas;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Lead;
using SolarES.Dominio.Simulacao;

namespace SolarES.Aplicacao.Leads;

public sealed class LeadAppService(ILeadRepository leads, ISimulacaoRepository simulacoes,
    IConfiguracaoVersaoRepository configuracoes, PropostaAppService propostas,
    IArmazenamentoAnexoConta armazenamentoAnexos, TimeProvider relogio)
{
    public async Task<DesfechoCapturaLead> CapturarPublicoAsync(Guid simulacaoId, string nome,
        string telefone, string email, CanalPreferido canal, bool consentimento,
        byte[]? conteudoAnexo, CancellationToken ct)
    {
        TipoAnexoConta? tipoAnexo = conteudoAnexo is null ? null : IdentificarTipoAnexo(conteudoAnexo);
        var simulacao = await simulacoes.ObterPorIdAsync(simulacaoId, ct);
        if (simulacao is null || simulacao.Origem != OrigemSimulacao.Landing)
            throw new KeyNotFoundException("Simulação pública não encontrada.");
        var entrada = JsonSerializer.Deserialize<EntradaSimulacao>(simulacao.EntradasSnapshot)
            ?? throw new InvalidOperationException("Entradas da simulação inválidas.");
        var municipioId = await leads.ObterMunicipioIdPorCodigoAsync(entrada.MunicipioCodigoIbge, ct)
            ?? throw new InvalidOperationException("Município da simulação não encontrado.");
        var lead = Lead.Criar(nome, telefone, email, canal, simulacao.Id, municipioId,
            consentimento, relogio.GetUtcNow());
        leads.Adicionar(lead);
        simulacao.LeadId = lead.Id;
        if (conteudoAnexo is not null && tipoAnexo is { } tipo)
        {
            var caminho = await armazenamentoAnexos.SalvarAsync(lead.Id, tipo, conteudoAnexo, ct);
            leads.AdicionarAnexo(AnexoConta.Criar(lead.Id, tipo, conteudoAnexo.LongLength,
                caminho, relogio.GetUtcNow()));
        }
        await leads.SalvarAlteracoesAsync(ct);

        var versao = await configuracoes.ObterPorIdAsync(simulacao.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versão da configuração não encontrada.");
        if (simulacao.RoteadaParaHumano) return DesfechoCapturaLead.RoteadoParaHumano;
        if (versao.Payload.PossuiPremissaProvisoria()) return DesfechoCapturaLead.CalibracaoPendente;
        await propostas.GerarAsync(simulacao.Id, ct);
        return DesfechoCapturaLead.PropostaEmitida;
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
