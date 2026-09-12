using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Metricas;
using SolarES.Dominio.Metricas;

namespace SolarES.Infraestrutura.Persistencia.Repositorios;

public sealed class EfEventoFunilRepository(SolarESDbContext contexto) : IEventoFunilRepository
{
    public void Adicionar(EventoFunil eventoFunil) => contexto.EventosFunil.Add(eventoFunil);
    public Task SalvarAlteracoesAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
    public async Task<MetricasFunilResultado> ObterMetricasAsync(DateTimeOffset de, DateTimeOffset ate,
        CancellationToken ct)
    {
        var periodo = contexto.EventosFunil.Where(e => e.OcorridoEm >= de && e.OcorridoEm <= ate);
        var iniciadas = await periodo.Where(e => e.Tipo == TipoEventoFunil.SimulacaoIniciada)
            .Select(e => e.SessaoFunilId).Distinct().CountAsync(ct);
        var concluidas = await periodo.Where(e => e.Tipo == TipoEventoFunil.SimulacaoConcluida)
            .Select(e => e.SessaoFunilId).Distinct().CountAsync(ct);
        var leadsCapturados = await periodo.Where(e => e.Tipo == TipoEventoFunil.LeadCapturado)
            .Select(e => e.SessaoFunilId).Distinct().CountAsync(ct);
        var anexosOferecidos = await periodo.Where(e => e.Tipo == TipoEventoFunil.AnexoOferecido)
            .Select(e => e.SessaoFunilId).Distinct().CountAsync(ct);
        return new(iniciadas, concluidas, leadsCapturados, anexosOferecidos);
    }
}
