namespace SolarES.Infraestrutura.Persistencia.Seeds;

internal sealed class TarifaVigenteSeedRegistro
{
    public required string SiglaDistribuidora { get; init; }
    public required string Subgrupo { get; init; }
    public decimal TarifaTe { get; init; }
    public decimal TarifaTusd { get; init; }
    public decimal ValorFioBPorKwh { get; init; }
    public decimal AliquotaIcms { get; init; }
    public decimal AliquotaPisCofins { get; init; }
    public DateTimeOffset VigenciaInicio { get; init; }
    public DateTimeOffset? VigenciaFim { get; init; }
    public required string ResolucaoHomologatoria { get; init; }
    public required string Fonte { get; init; }
}
