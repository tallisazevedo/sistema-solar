using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Aplicacao.Propostas;

public interface IPropostaRepository
{
    Task<int> ContarAsync(CancellationToken ct);
    Task<PropostaEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct);
    void Adicionar(PropostaEntidade proposta);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
