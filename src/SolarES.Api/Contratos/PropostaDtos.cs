using SolarES.Dominio.Proposta;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Api.Contratos;

public sealed record PropostaResponse(Guid Id, Guid SimulacaoId, string Numero, DateTimeOffset ValidaAte,
    StatusProposta Status, DateTimeOffset? EnviadaEm, CanalEnvio? Canal,
    DateTimeOffset? AceitaEm, DateTimeOffset? PerdidaEm, string? MotivoPerda, bool CalibracaoPendente)
{
    public static PropostaResponse DeEntidade(PropostaEntidade p, bool calibracaoPendente) => new(p.Id, p.SimulacaoId,
        p.Numero, p.ValidaAte, p.Status, p.EnviadaEm, p.Canal, p.AceitaEm, p.PerdidaEm, p.MotivoPerda, calibracaoPendente);
}
public sealed record MarcarPerdidaRequest(string? Motivo);
