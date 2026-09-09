namespace SolarES.Dominio.Lead;

public sealed class Lead : EntidadeBase
{
    public required string Nome { get; set; }
    public required string Telefone { get; set; }
    public required string Email { get; set; }
    public required string Origem { get; set; }
    public Guid MunicipioId { get; set; }
    public DateTimeOffset? ConsentimentoLgpdEm { get; set; }
    public required string Status { get; set; }
}
