namespace SolarES.Dominio.Catalogo;

public sealed class Inversor : EntidadeBase
{
    public required string Fabricante { get; set; }
    public required string Modelo { get; set; }
    public int PotenciaW { get; set; }
    public int QuantidadeMppt { get; set; }
    public TipoInversor Tipo { get; set; }
    public bool Ativo { get; set; }
}
