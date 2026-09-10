using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Simulacoes;
using SimulacaoEntidade = SolarES.Dominio.Simulacao.Simulacao;

namespace SolarES.Infraestrutura.Persistencia.Repositorios;

public sealed class EfSimulacaoRepository(SolarESDbContext contexto) : ISimulacaoRepository
{
    public async Task<IReadOnlyList<SimulacaoEntidade>> ListarMaisRecentesAsync(CancellationToken ct) =>
        await contexto.Simulacoes.OrderByDescending(s => s.CriadoEm).ToListAsync(ct);

    public Task<SimulacaoEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        contexto.Simulacoes.SingleOrDefaultAsync(s => s.Id == id, ct);

    public void Adicionar(SimulacaoEntidade simulacao) => contexto.Simulacoes.Add(simulacao);

    public Task SalvarAlteracoesAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
