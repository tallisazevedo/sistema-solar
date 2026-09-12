using Hangfire;

namespace SolarES.Aplicacao.Leads;

/// <summary>
/// Job recorrente do Hangfire (T24) que apaga do armazenamento os anexos de conta cujo
/// prazo de guarda venceu. O registro do anexo permanece, com DescartadoEm preenchido --
/// so' o arquivo some.
/// </summary>
public sealed class DescartarAnexosVencidosJob(ILeadRepository leads,
    IArmazenamentoAnexoConta armazenamentoAnexos, TimeProvider relogio)
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecutarAsync(CancellationToken ct)
    {
        var agora = relogio.GetUtcNow();
        var vencidos = await leads.ListarAnexosParaDescarteAsync(agora, ct);
        foreach (var anexo in vencidos)
        {
            await armazenamentoAnexos.ApagarAsync(anexo.CaminhoArmazenamento, ct);
            anexo.Descartar(agora);
        }
        if (vencidos.Count > 0) await leads.SalvarAlteracoesAsync(ct);
    }
}
