using System.Text.Json;
using SolarES.Aplicacao.Configuracao;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Lead;
using SolarES.Dominio.Simulacao;

namespace SolarES.Aplicacao.Leads;

public sealed class ConsultaLeadsAppService(ILeadRepository leads, ISimulacaoRepository simulacoes,
    IConfiguracaoVersaoRepository configuracoes, IArmazenamentoAnexoConta armazenamentoAnexos)
{
    public async Task<IReadOnlyList<LeadAdministrativoResultado>> ListarAsync(OrigemLead? origem, StatusLead? status,
        CancellationToken ct)
    {
        var encontrados = await leads.ListarAsync(origem, status, ct);
        var resultados = new List<LeadAdministrativoResultado>(encontrados.Count);
        foreach (var lead in encontrados)
        {
            var resultado = await MontarAsync(lead, ct);
            if (resultado is not null) resultados.Add(resultado);
        }
        return resultados;
    }

    public async Task<LeadAdministrativoResultado?> ObterAsync(Guid id, CancellationToken ct)
    {
        var lead = await leads.ObterPorIdAsync(id, ct);
        return lead is null ? null : await MontarAsync(lead, ct);
    }

    public async Task<AnexoContaDownload?> ObterAnexoAsync(Guid leadId, CancellationToken ct)
    {
        if (await leads.ObterPorIdAsync(leadId, ct) is null) return null;
        var anexo = await leads.ObterAnexoAsync(leadId, ct);
        if (anexo is null) return null;
        var conteudo = await armazenamentoAnexos.LerAsync(anexo.CaminhoArmazenamento, ct);
        return conteudo is null ? null : new(conteudo, anexo.Tipo);
    }

    private async Task<LeadAdministrativoResultado?> MontarAsync(Lead lead, CancellationToken ct)
    {
        if (lead.SimulacaoId is not { } simulacaoId)
        {
            var consentimentosManuais = await leads.ListarConsentimentosAsync(lead.Id, ct);
            return new(lead.Id, lead.Nome, lead.Telefone, lead.Email, lead.CanalPreferido,
                lead.Status, lead.Origem, lead.VisitaTecnicaAgendadaPara, lead.CriadoEm, false, false, false, null, null,
                consentimentosManuais.Select(c => new ConsentimentoAdministrativoResultado(
                    c.Finalidade, c.VersaoTexto, c.ConcedidoEm)).ToList());
        }
        var simulacao = await simulacoes.ObterPorIdAsync(simulacaoId, ct);
        if (simulacao is null) return null;
        var versao = await configuracoes.ObterPorIdAsync(simulacao.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versão da configuração não encontrada.");
        var resultado = JsonSerializer.Deserialize<ResultadoSimulacao>(simulacao.ResultadoSnapshot)
            ?? throw new InvalidOperationException("Resultado da simulação inválido.");
        var possuiAnexo = await leads.ObterAnexoAsync(lead.Id, ct) is not null;
        var consentimentos = await leads.ListarConsentimentosAsync(lead.Id, ct);
        return new(lead.Id, lead.Nome, lead.Telefone, lead.Email, lead.CanalPreferido,
            lead.Status, lead.Origem, lead.VisitaTecnicaAgendadaPara, lead.CriadoEm, simulacao.RoteadaParaHumano,
            versao.Payload.PossuiPremissaProvisoria(), possuiAnexo, simulacao.Id, resultado,
            consentimentos.Select(c => new ConsentimentoAdministrativoResultado(
                c.Finalidade, c.VersaoTexto, c.ConcedidoEm)).ToList());
    }
}

public sealed record LeadAdministrativoResultado(Guid Id, string Nome, string Telefone, string Email,
    CanalPreferido? CanalPreferido, StatusLead Status, OrigemLead Origem,
    DateTimeOffset? VisitaTecnicaAgendadaPara, DateTimeOffset CriadoEm,
    bool RoteadoParaHumano, bool CalibracaoPendente, bool PossuiAnexo,
    Guid? SimulacaoId, ResultadoSimulacao? Resultado,
    IReadOnlyList<ConsentimentoAdministrativoResultado> Consentimentos);
public sealed record ConsentimentoAdministrativoResultado(FinalidadeConsentimento Finalidade,
    string VersaoTexto, DateTimeOffset ConcedidoEm);
public sealed record AnexoContaDownload(byte[] Conteudo, TipoAnexoConta Tipo);
