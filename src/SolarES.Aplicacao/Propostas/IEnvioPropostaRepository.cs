using SolarES.Dominio.Proposta;

namespace SolarES.Aplicacao.Propostas;

public interface IEnvioPropostaRepository
{
    Task<EnvioProposta?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<EnvioProposta?> ObterPorIdMensagemProvedorAsync(string idMensagemProvedor, CancellationToken ct);
    Task<IReadOnlyList<EnvioProposta>> ListarPorPropostaAsync(Guid propostaId, CancellationToken ct);
    void Adicionar(EnvioProposta envio);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
