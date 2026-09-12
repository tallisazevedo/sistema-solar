namespace SolarES.Dominio.Lead;

public sealed class AnexoConta : EntidadeBase
{
    private AnexoConta() { }
    public Guid LeadId { get; private set; }
    public TipoAnexoConta Tipo { get; private set; }
    public long Tamanho { get; private set; }
    public DateTimeOffset RecebidoEm { get; private set; }
    public string CaminhoArmazenamento { get; private set; } = null!;
    public DateTimeOffset DescartarAte { get; private set; }
    public DateTimeOffset? DescartadoEm { get; private set; }

    public static AnexoConta Criar(Guid leadId, TipoAnexoConta tipo, long tamanho,
        string caminhoArmazenamento, DateTimeOffset recebidoEm, DateTimeOffset descartarAte)
    {
        if (leadId == Guid.Empty) throw new ArgumentException("Lead do anexo é obrigatório.");
        if (!Enum.IsDefined(tipo)) throw new ArgumentException("Tipo de anexo inválido.");
        if (tamanho <= 0) throw new ArgumentException("O anexo não pode estar vazio.");
        if (string.IsNullOrWhiteSpace(caminhoArmazenamento)) throw new ArgumentException("Caminho do anexo é obrigatório.");
        if (descartarAte <= recebidoEm) throw new ArgumentException("Prazo de descarte deve ser posterior ao recebimento.");
        return new()
        {
            Id = Guid.NewGuid(), LeadId = leadId, Tipo = tipo, Tamanho = tamanho,
            CaminhoArmazenamento = caminhoArmazenamento, RecebidoEm = recebidoEm,
            DescartarAte = descartarAte, CriadoEm = recebidoEm, AtualizadoEm = recebidoEm,
        };
    }

    /// <summary>Remove o arquivo do armazenamento; o registro fica, sem o conteudo.</summary>
    public void Descartar(DateTimeOffset momento)
    {
        if (DescartadoEm is not null) throw new InvalidOperationException("Anexo já foi descartado.");
        DescartadoEm = momento;
        AtualizadoEm = momento;
    }
}
