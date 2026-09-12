using SolarES.Dominio.Metricas;

namespace SolarES.Aplicacao.Metricas;

public sealed class FunilAppService(IEventoFunilRepository eventos, TimeProvider relogio)
{
    public async Task RegistrarInicioAsync(Guid sessaoFunilId, CancellationToken ct)
    {
        eventos.Adicionar(EventoFunil.CriarInicio(sessaoFunilId, relogio.GetUtcNow()));
        await eventos.SalvarAlteracoesAsync(ct);
    }

    public async Task RegistrarConclusaoAsync(Guid sessaoFunilId, Guid simulacaoId, CancellationToken ct)
    {
        eventos.Adicionar(EventoFunil.CriarConclusao(sessaoFunilId, simulacaoId, relogio.GetUtcNow()));
        await eventos.SalvarAlteracoesAsync(ct);
    }

    public Task<MetricasFunilResultado> ObterAsync(DateTimeOffset de, DateTimeOffset ate, CancellationToken ct)
    {
        if (ate < de) throw new ArgumentException("A data final deve ser posterior a data inicial.");
        return eventos.ObterMetricasAsync(de, ate, ct);
    }
}
