namespace SolarES.Dominio.Simulacao;

public sealed class Simulacao : EntidadeBase
{
    public Guid? LeadId { get; set; }
    public Guid? ContaExtraidaId { get; set; }
    public Guid ConfiguracaoVersaoId { get; set; }
    public OrigemSimulacao Origem { get; set; } = OrigemSimulacao.Interna;

    /// <summary>Snapshot serializado da EntradaSimulacao usada para gerar este resultado.</summary>
    public required string EntradasSnapshot { get; set; }

    /// <summary>Snapshot serializado do ResultadoSimulacao produzido pelo motor.</summary>
    public required string ResultadoSnapshot { get; set; }

    public decimal PotenciaKwp { get; set; }
    public int QuantidadeModulos { get; set; }
    public decimal Capex { get; set; }
    public decimal EconomiaMensalAno1 { get; set; }
    public int? PaybackMeses { get; set; }
    public decimal? Tir { get; set; }
    public decimal Vpl { get; set; }
    public decimal CoberturaPercentual { get; set; }
    public bool RoteadaParaHumano { get; set; }
    public string? MotivoRoteamento { get; set; }
}
