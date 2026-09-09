namespace SolarES.Dominio.Simulacao;

public sealed record ResultadoFinanceiro(
    int? PaybackMesesSimples,
    int? PaybackMesesDescontado,
    decimal Vpl,
    decimal? Tir);
