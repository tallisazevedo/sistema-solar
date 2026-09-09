namespace SolarES.Dominio.Catalogo;

public sealed class ModuloFotovoltaico : EntidadeBase
{
    public required string Fabricante { get; set; }
    public required string Modelo { get; set; }
    public int PotenciaW { get; set; }
    public int LarguraMm { get; set; }
    public int AlturaMm { get; set; }
    public decimal EficienciaPercentual { get; set; }
    public bool ResistenteNevoaSalina { get; set; }
    public bool Ativo { get; set; }
}
