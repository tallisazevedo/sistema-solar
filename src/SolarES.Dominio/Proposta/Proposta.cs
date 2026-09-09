namespace SolarES.Dominio.Proposta;

public sealed class Proposta : EntidadeBase
{
    public Guid SimulacaoId { get; set; }
    public required string Numero { get; set; }
    public Guid ConfiguracaoVersaoId { get; set; }
    public DateTimeOffset ValidaAte { get; set; }
    public string? ArquivoPdfUrl { get; set; }
    public DateTimeOffset? EnviadaEm { get; set; }
    public CanalEnvio? Canal { get; set; }
    public StatusProposta Status { get; set; }
}
