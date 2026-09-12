using SolarES.Dominio.Metricas;

namespace SolarES.Aplicacao.Metricas;

public interface IEventoFunilRepository
{
    void Adicionar(EventoFunil eventoFunil);
    Task SalvarAlteracoesAsync(CancellationToken ct);
    Task<MetricasFunilResultado> ObterMetricasAsync(DateTimeOffset de, DateTimeOffset ate, CancellationToken ct);
}

public sealed record MetricasFunilResultado(int SessoesIniciadas, int SessoesConcluidas,
    int LeadsCapturados, int AnexosOferecidos)
{
    public decimal TaxaConclusaoPercentual => SessoesIniciadas == 0
        ? 0 : decimal.Round(SessoesConcluidas * 100m / SessoesIniciadas, 2);
}
