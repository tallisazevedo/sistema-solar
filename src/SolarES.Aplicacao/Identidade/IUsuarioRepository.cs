using SolarES.Dominio.Identidade;

namespace SolarES.Aplicacao.Identidade;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct);
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct);
    void Adicionar(Usuario usuario);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
