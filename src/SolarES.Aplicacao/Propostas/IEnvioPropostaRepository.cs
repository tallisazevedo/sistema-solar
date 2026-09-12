using SolarES.Dominio.Proposta;

namespace SolarES.Aplicacao.Propostas;

public interface IEnvioPropostaRepository
{
    Task<EnvioProposta?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<EnvioProposta?> ObterPorIdMensagemProvedorAsync(string idMensagemProvedor, CancellationToken ct);
    Task<IReadOnlyList<EnvioProposta>> ListarPorPropostaAsync(Guid propostaId, CancellationToken ct);
    void Adicionar(EnvioProposta envio);
    /// <summary>Envios que efetivamente chegaram ao canal (Enviado/Entregue) para o destino normalizado, desde o momento informado.</summary>
    Task<int> ContarEnviadosPorDestinoDesdeAsync(string destinoNormalizado, DateTimeOffset desde, CancellationToken ct);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
