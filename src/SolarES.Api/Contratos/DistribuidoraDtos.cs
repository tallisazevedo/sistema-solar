using System.ComponentModel.DataAnnotations;
using SolarES.Dominio.Tarifas;

namespace SolarES.Api.Contratos;

public sealed record DistribuidoraRequest(
    [Required, MaxLength(200)] string Nome,
    [Required, MaxLength(20)] string SiglaAneel,
    bool Ativa)
{
    public Distribuidora ParaEntidade(Guid id) => new()
    {
        Id = id,
        Nome = Nome,
        SiglaAneel = SiglaAneel,
        Ativa = Ativa,
    };
}

public sealed record DistribuidoraResponse(Guid Id, string Nome, string SiglaAneel, bool Ativa)
{
    public static DistribuidoraResponse DeEntidade(Distribuidora entidade) => new(
        entidade.Id, entidade.Nome, entidade.SiglaAneel, entidade.Ativa);
}
