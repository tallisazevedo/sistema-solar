namespace SolarES.Dominio.Tarifas;

public sealed class Distribuidora : EntidadeBase
{
    public required string Nome { get; set; }
    public required string SiglaAneel { get; set; }
    public bool Ativa { get; set; }
}
