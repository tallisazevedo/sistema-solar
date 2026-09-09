namespace SolarES.Dominio.Simulacao;

/// <summary>Ano vai de 1 (primeiro ano) ate HorizonteAnos.</summary>
public sealed record AnoProjecao(
    int Ano,
    int AnoCalendario,
    decimal GeracaoAnualKwh,
    decimal EconomiaLiquidaAnualReais,
    decimal EconomiaLiquidaAnualEmTermosReaisReais);
