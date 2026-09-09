namespace SolarES.Infraestrutura.Persistencia.Seeds;

internal sealed class MunicipioSeedRegistro
{
    public required string CodigoIbge { get; init; }
    public required string Nome { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public decimal DistanciaMarKm { get; init; }
    public required IReadOnlyList<decimal> HspPorMes { get; init; }
    public required string SiglaDistribuidora { get; init; }
}
