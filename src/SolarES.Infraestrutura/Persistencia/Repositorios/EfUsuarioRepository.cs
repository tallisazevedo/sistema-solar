using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Identidade;
using SolarES.Dominio.Identidade;

namespace SolarES.Infraestrutura.Persistencia.Repositorios;

public sealed class EfUsuarioRepository(SolarESDbContext contexto) : IUsuarioRepository
{
    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct) =>
        contexto.Usuarios.SingleOrDefaultAsync(u => u.Email == email, ct);

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        contexto.Usuarios.SingleOrDefaultAsync(u => u.Id == id, ct);

    public void Adicionar(Usuario usuario) => contexto.Usuarios.Add(usuario);

    public Task SalvarAlteracoesAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
