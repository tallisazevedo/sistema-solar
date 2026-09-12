namespace SolarES.Dominio.Lead;

public sealed class HistoricoStatusLead : EntidadeBase
{
    private HistoricoStatusLead() { }
    public Guid LeadId { get; private set; }
    public StatusLead StatusAnterior { get; private set; }
    public StatusLead StatusNovo { get; private set; }
    public Guid UsuarioId { get; private set; }
    public DateTimeOffset AlteradoEm { get; private set; }

    internal static HistoricoStatusLead Criar(Guid leadId, StatusLead anterior, StatusLead novo,
        Guid usuarioId, DateTimeOffset momento) => new()
        {
            Id = Guid.NewGuid(),
            LeadId = leadId,
            StatusAnterior = anterior,
            StatusNovo = novo,
            UsuarioId = usuarioId,
            AlteradoEm = momento,
            CriadoEm = momento,
            AtualizadoEm = momento,
        };
}
