namespace SolarES.Dominio.Lead;

public sealed class ConsentimentoLgpd : EntidadeBase
{
    private ConsentimentoLgpd() { }
    public Guid LeadId { get; private set; }
    public FinalidadeConsentimento Finalidade { get; private set; }
    public string VersaoTexto { get; private set; } = null!;
    public DateTimeOffset ConcedidoEm { get; private set; }

    public static ConsentimentoLgpd Criar(Guid leadId, FinalidadeConsentimento finalidade,
        string versaoTexto, DateTimeOffset concedidoEm)
    {
        if (leadId == Guid.Empty) throw new ArgumentException("Lead do consentimento é obrigatório.");
        if (!Enum.IsDefined(finalidade)) throw new ArgumentException("Finalidade de consentimento inválida.");
        if (string.IsNullOrWhiteSpace(versaoTexto)) throw new ArgumentException("Versão do texto é obrigatória.");
        return new() { Id = Guid.NewGuid(), LeadId = leadId, Finalidade = finalidade,
            VersaoTexto = versaoTexto, ConcedidoEm = concedidoEm, CriadoEm = concedidoEm, AtualizadoEm = concedidoEm };
    }
}
