using SolarES.Dominio.Proposta;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Api.Contratos;

public sealed record PropostaResponse(Guid Id, Guid SimulacaoId, string Numero, DateTimeOffset ValidaAte, StatusProposta Status)
{
    public static PropostaResponse DeEntidade(PropostaEntidade p) => new(p.Id, p.SimulacaoId, p.Numero, p.ValidaAte, p.Status);
}
