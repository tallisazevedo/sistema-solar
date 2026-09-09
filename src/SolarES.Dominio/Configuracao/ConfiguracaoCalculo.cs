using SolarES.Dominio.Premissas;

namespace SolarES.Dominio.Configuracao;

public sealed record ConfiguracaoCalculo(
    Premissa<decimal> PerformanceRatio,
    Premissa<decimal> DegradacaoAnual,
    Premissa<decimal> InflacaoTarifaria,
    Premissa<decimal> TaxaDesconto,
    Premissa<int> HorizonteAnos,
    Premissa<decimal> OversizingMaximo,
    Premissa<decimal> FatorOrientacaoPadrao,
    Premissa<IReadOnlyList<PercentualFioBAno>> CronogramaFioB,
    Premissa<EstrategiaFioBForaCronograma> EstrategiaFioBForaCronograma,
    Premissa<decimal> LimiteKwpRoteamentoHumano,
    Premissa<ConfiguracaoKitLitoral> KitLitoral);
