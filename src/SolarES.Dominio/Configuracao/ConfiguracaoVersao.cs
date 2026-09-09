namespace SolarES.Dominio.Configuracao;

public sealed class ConfiguracaoVersao : EntidadeBase
{
    public int Numero { get; set; }
    public StatusConfiguracaoVersao Status { get; set; }
    public required ConfiguracaoCalculo Payload { get; set; }
    public DateTimeOffset? PublicadaEm { get; set; }
    public Guid? PublicadaPorUsuarioId { get; set; }
    public string? Observacao { get; set; }
}
