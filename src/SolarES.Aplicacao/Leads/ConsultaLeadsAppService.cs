using System.Text.Json;
using SolarES.Aplicacao.Configuracao;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Lead;
using SolarES.Dominio.Simulacao;

namespace SolarES.Aplicacao.Leads;

public sealed class ConsultaLeadsAppService(ILeadRepository leads, ISimulacaoRepository simulacoes,
    IConfiguracaoVersaoRepository configuracoes)
{
    public async Task<IReadOnlyList<LeadAdministrativoResultado>> ListarAsync(CancellationToken ct)
    {
        var encontrados = await leads.ListarDaLandingAsync(ct);
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

    private async Task<LeadAdministrativoResultado?> MontarAsync(Lead lead, CancellationToken ct)
    {
        if (lead.SimulacaoId is not { } simulacaoId) return null;
        var simulacao = await simulacoes.ObterPorIdAsync(simulacaoId, ct);
        if (simulacao is null) return null;
        var versao = await configuracoes.ObterPorIdAsync(simulacao.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versão da configuração não encontrada.");
        var resultado = JsonSerializer.Deserialize<ResultadoSimulacao>(simulacao.ResultadoSnapshot)
            ?? throw new InvalidOperationException("Resultado da simulação inválido.");
        return new(lead.Id, lead.Nome, lead.Telefone, lead.Email, lead.CanalPreferido,
            lead.Status, lead.CriadoEm, simulacao.RoteadaParaHumano,
            versao.Payload.PossuiPremissaProvisoria(), simulacao.Id, resultado);
    }
}

public sealed record LeadAdministrativoResultado(Guid Id, string Nome, string Telefone, string Email,
    CanalPreferido? CanalPreferido, StatusLead Status, DateTimeOffset CriadoEm,
    bool RoteadoParaHumano, bool CalibracaoPendente, Guid SimulacaoId, ResultadoSimulacao Resultado);
