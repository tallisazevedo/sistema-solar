using SimulacaoEntidade = SolarES.Dominio.Simulacao.Simulacao;

namespace SolarES.Aplicacao.Simulacoes;

public interface ISimulacaoRepository
{
    Task<IReadOnlyList<SimulacaoEntidade>> ListarMaisRecentesAsync(CancellationToken ct);
    Task<SimulacaoEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct);
    void Adicionar(SimulacaoEntidade simulacao);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
