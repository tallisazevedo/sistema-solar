using SolarES.Dominio.Proposta;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Aplicacao.Propostas;

public interface IPropostaRepository
{
    Task<int> ContarAsync(CancellationToken ct);
    Task<PropostaEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<PropostaEntidade>> ListarAsync(StatusProposta? status, CancellationToken ct);
    Task<IReadOnlyList<PropostaEntidade>> ListarPendentesDeVencimentoAsync(DateTimeOffset agora, CancellationToken ct);
    void Adicionar(PropostaEntidade proposta);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
