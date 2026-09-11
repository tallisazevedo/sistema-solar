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

    /// <summary>Dados do titular legiveis por humano, para atender pedido de acesso via LGPD.</summary>
    public async Task<ExportacaoLeadResultado?> ObterExportacaoAsync(Guid leadId, CancellationToken ct)
    {
        var lead = await leads.ObterPorIdAsync(leadId, ct);
        if (lead is null) return null;
        var consentimentos = await leads.ListarConsentimentosAsync(lead.Id, ct);
        var anexo = await leads.ObterAnexoAsync(lead.Id, ct);
        return new(lead.Id, lead.Nome, lead.Telefone, lead.Email, lead.Status, lead.CriadoEm,
            lead.ExpurgadoEm, lead.SimulacaoId,
            consentimentos.Select(c => new ConsentimentoAdministrativoResultado(
                c.Finalidade, c.VersaoTexto, c.ConcedidoEm)).ToList(),
            anexo is null ? null : new AnexoMetadadoResultado(
                anexo.Tipo, anexo.Tamanho, anexo.RecebidoEm, anexo.DescartarAte, anexo.DescartadoEm));
    }

    private async Task<LeadAdministrativoResultado?> MontarAsync(Lead lead, CancellationToken ct)
    {
        if (lead.SimulacaoId is not { } simulacaoId)
        {
            var consentimentosManuais = await leads.ListarConsentimentosAsync(lead.Id, ct);
            return new(lead.Id, lead.Nome, lead.Telefone, lead.Email, lead.CanalPreferido,
                lead.Status, lead.Origem, lead.VisitaTecnicaAgendadaPara, lead.CriadoEm, false, false, false, null, null,
                consentimentosManuais.Select(c => new ConsentimentoAdministrativoResultado(
                    c.Finalidade, c.VersaoTexto, c.ConcedidoEm)).ToList(), lead.ExpurgadoEm);
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
                c.Finalidade, c.VersaoTexto, c.ConcedidoEm)).ToList(), lead.ExpurgadoEm);
    }
}

public sealed record LeadAdministrativoResultado(Guid Id, string Nome, string Telefone, string Email,
    CanalPreferido? CanalPreferido, StatusLead Status, OrigemLead Origem,
    DateTimeOffset? VisitaTecnicaAgendadaPara, DateTimeOffset CriadoEm,
    bool RoteadoParaHumano, bool CalibracaoPendente, bool PossuiAnexo,
    Guid? SimulacaoId, ResultadoSimulacao? Resultado,
    IReadOnlyList<ConsentimentoAdministrativoResultado> Consentimentos, DateTimeOffset? ExpurgadoEm);
public sealed record ConsentimentoAdministrativoResultado(FinalidadeConsentimento Finalidade,
    string VersaoTexto, DateTimeOffset ConcedidoEm);
public sealed record AnexoContaDownload(byte[] Conteudo, TipoAnexoConta Tipo);
public sealed record ExportacaoLeadResultado(Guid Id, string Nome, string Telefone, string Email,
    StatusLead Status, DateTimeOffset CriadoEm, DateTimeOffset? ExpurgadoEm, Guid? SimulacaoId,
    IReadOnlyList<ConsentimentoAdministrativoResultado> Consentimentos, AnexoMetadadoResultado? Anexo);
public sealed record AnexoMetadadoResultado(TipoAnexoConta Tipo, long Tamanho, DateTimeOffset RecebidoEm,
    DateTimeOffset DescartarAte, DateTimeOffset? DescartadoEm);
