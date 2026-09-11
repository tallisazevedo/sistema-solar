using Hangfire;

namespace SolarES.Aplicacao.Leads;

/// <summary>
/// Job recorrente do Hangfire (T24) que anonimiza leads criados ha mais que o prazo
/// configurado e que nunca chegaram a Convertido. Simulacao e consentimentos
/// permanecem -- so o dado pessoal do lead e o anexo de conta somem.
/// </summary>
public sealed class ExpurgarLeadsInativosJob(ILeadRepository leads,
    IArmazenamentoAnexoConta armazenamentoAnexos, ConfiguracaoRetencaoLgpd configuracaoRetencao,
    TimeProvider relogio)
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecutarAsync(CancellationToken ct)
    {
        var agora = relogio.GetUtcNow();
        var limite = agora.AddMonths(-configuracaoRetencao.PrazoExpurgoLeadMeses);
        var candidatos = await leads.ListarParaExpurgoAsync(limite, ct);
        foreach (var lead in candidatos)
        {
            var anexo = await leads.ObterAnexoAsync(lead.Id, ct);
            await AnonimizacaoLead.ExecutarAsync(lead, anexo, armazenamentoAnexos, agora, ct);
        }
        if (candidatos.Count > 0) await leads.SalvarAlteracoesAsync(ct);
    }
}
