namespace SolarES.Dominio.Tarifas;

public sealed class MunicipioHsp : EntidadeBase
{
    public required string CodigoIbge { get; set; }
    public required string Nome { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal DistanciaMarKm { get; set; }
    public Guid DistribuidoraId { get; set; }

    /// <summary>Doze valores de HSP, um por mes, na ordem janeiro a dezembro.</summary>
    public required IReadOnlyList<decimal> HspPorMes { get; set; }

    public required string Fonte { get; set; }
}
