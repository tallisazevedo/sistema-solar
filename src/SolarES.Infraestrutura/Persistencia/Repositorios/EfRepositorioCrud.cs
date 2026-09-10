using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Dominio;

namespace SolarES.Infraestrutura.Persistencia.Repositorios;

public sealed class EfRepositorioCrud<TEntidade>(SolarESDbContext contexto) : IRepositorioCrud<TEntidade>
    where TEntidade : EntidadeBase
{
    public async Task<IReadOnlyList<TEntidade>> ListarAsync(CancellationToken ct) =>
        await contexto.Set<TEntidade>().ToListAsync(ct);

    public Task<TEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        contexto.Set<TEntidade>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public void Adicionar(TEntidade entidade) => contexto.Set<TEntidade>().Add(entidade);

    public void Remover(TEntidade entidade) => contexto.Set<TEntidade>().Remove(entidade);

    public Task SalvarAlteracoesAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
