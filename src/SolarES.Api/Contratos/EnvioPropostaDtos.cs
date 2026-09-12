using SolarES.Dominio.Proposta;

namespace SolarES.Api.Contratos;

public sealed record SolicitarEnvioPropostaRequest(CanalEnvio Canal, string Destino);

public sealed record EnvioPropostaResponse(Guid Id, Guid PropostaId, CanalEnvio Canal, string Destino,
    StatusEnvioProposta Status, string? IdMensagemProvedor, int Tentativas, string? UltimoErro,
    DateTimeOffset? EnviadoEm, DateTimeOffset? EntregueEm, DateTimeOffset CriadoEm)
{
    public static EnvioPropostaResponse DeEntidade(EnvioProposta e) => new(e.Id, e.PropostaId, e.Canal, e.Destino,
        e.Status, e.IdMensagemProvedor, e.Tentativas, e.UltimoErro, e.EnviadoEm, e.EntregueEm, e.CriadoEm);
}
