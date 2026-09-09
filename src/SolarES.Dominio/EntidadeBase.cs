namespace SolarES.Dominio;

public abstract class EntidadeBase
{
    public Guid Id { get; set; }

    /// <summary>Reservado para isolamento multi-tenant. Nao usado no MVP.</summary>
    public Guid TenantId { get; set; }

    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
}
