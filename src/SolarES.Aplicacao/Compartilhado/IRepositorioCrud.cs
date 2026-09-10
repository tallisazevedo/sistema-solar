using SolarES.Dominio;

namespace SolarES.Aplicacao.Compartilhado;

public interface IRepositorioCrud<TEntidade> where TEntidade : EntidadeBase
{
    Task<IReadOnlyList<TEntidade>> ListarAsync(CancellationToken ct);
    Task<TEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct);
    void Adicionar(TEntidade entidade);
    void Remover(TEntidade entidade);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
