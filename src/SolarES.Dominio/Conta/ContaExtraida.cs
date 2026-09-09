using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Conta;

public sealed class ContaExtraida : EntidadeBase
{
    public Guid LeadId { get; set; }
    public Guid DistribuidoraId { get; set; }
    public required string NumeroUc { get; set; }
    public Subgrupo Subgrupo { get; set; }
    public TipoLigacao TipoLigacao { get; set; }

    /// <summary>Doze valores de consumo em kWh, um por mes, na ordem janeiro a dezembro.</summary>
    public required IReadOnlyList<decimal> HistoricoConsumo { get; set; }

    public decimal TarifaExtraida { get; set; }
    public MetodoExtracao MetodoExtracao { get; set; }
    public required string PayloadBruto { get; set; }
    public DateTimeOffset? ConfirmadaPeloClienteEm { get; set; }
    public DateTimeOffset? ImagemDescartadaEm { get; set; }
}
