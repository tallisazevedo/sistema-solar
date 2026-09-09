namespace SolarES.Dominio.Precificacao;

public sealed class FaixaPreco : EntidadeBase
{
    public decimal KwpMinimo { get; set; }
    public decimal KwpMaximo { get; set; }
    public decimal PrecoPorWp { get; set; }
    public required string TipoInstalacao { get; set; }
    public bool KitLitoral { get; set; }
    public DateTimeOffset Vigencia { get; set; }
}
