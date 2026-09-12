using SolarES.Aplicacao.Leads;
using SolarES.Dominio.Metricas;

namespace SolarES.Aplicacao.Metricas;

public sealed class FunilAppService(IEventoFunilRepository eventos, ILeadRepository leads, TimeProvider relogio)
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

    public async Task RegistrarLeadCapturadoAsync(Guid sessaoFunilId, CancellationToken ct)
    {
        eventos.Adicionar(EventoFunil.CriarLeadCapturado(sessaoFunilId, relogio.GetUtcNow()));
        await eventos.SalvarAlteracoesAsync(ct);
    }

    public async Task RegistrarAnexoOferecidoAsync(Guid sessaoFunilId, CancellationToken ct)
    {
        eventos.Adicionar(EventoFunil.CriarAnexoOferecido(sessaoFunilId, relogio.GetUtcNow()));
        await eventos.SalvarAlteracoesAsync(ct);
    }

    public async Task<MetricasFunilCompletoResultado> ObterAsync(DateTimeOffset de, DateTimeOffset ate,
        CancellationToken ct)
    {
        if (ate < de) throw new ArgumentException("A data final deve ser posterior a data inicial.");
        var funil = await eventos.ObterMetricasAsync(de, ate, ct);
        var anexoLanding = await leads.ObterMetricasAnexoLandingAsync(de, ate, ct);
        var conversaoPorOrigem = await leads.ObterConversaoPorOrigemAsync(de, ate, ct);
        return new(funil, anexoLanding, conversaoPorOrigem);
    }
}

public sealed record MetricasFunilCompletoResultado(MetricasFunilResultado Funil,
    MetricasAnexoLandingResultado AnexoLanding, IReadOnlyList<ConversaoOrigemResultado> ConversaoPorOrigem);
