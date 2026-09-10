using System.ComponentModel.DataAnnotations;
using SolarES.Dominio.Tarifas;

namespace SolarES.Api.Contratos;

public sealed record MunicipioHspRequest(
    [Required, MaxLength(7)] string CodigoIbge,
    [Required, MaxLength(200)] string Nome,
    decimal Latitude,
    decimal Longitude,
    [Range(0, double.MaxValue)] decimal DistanciaMarKm,
    [Required] Guid DistribuidoraId,
    [Required, MinLength(12), MaxLength(12)] List<decimal> HspPorMes,
    [Required, MaxLength(500)] string Fonte)
{
    public MunicipioHsp ParaEntidade(Guid id) => new()
    {
        Id = id,
        CodigoIbge = CodigoIbge,
        Nome = Nome,
        Latitude = Latitude,
        Longitude = Longitude,
        DistanciaMarKm = DistanciaMarKm,
        DistribuidoraId = DistribuidoraId,
        HspPorMes = HspPorMes,
        Fonte = Fonte,
    };
}

public sealed record MunicipioHspResponse(
    Guid Id,
    string CodigoIbge,
    string Nome,
    decimal Latitude,
    decimal Longitude,
    decimal DistanciaMarKm,
    Guid DistribuidoraId,
    IReadOnlyList<decimal> HspPorMes,
    string Fonte)
{
    public static MunicipioHspResponse DeEntidade(MunicipioHsp entidade) => new(
        entidade.Id, entidade.CodigoIbge, entidade.Nome, entidade.Latitude, entidade.Longitude,
        entidade.DistanciaMarKm, entidade.DistribuidoraId, entidade.HspPorMes, entidade.Fonte);
}
