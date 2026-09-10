namespace SolarES.Dominio.Simulacao;

public sealed record ResultadoSimulacao(
    decimal PotenciaInstaladaKwp,
    int QuantidadeModulos,
    decimal AreaNecessariaM2,
    decimal CoberturaPercentual,
    decimal Capex,
    decimal EconomiaMensalAno1,
    int? PaybackMesesSimples,
    int? PaybackMesesDescontado,
    decimal? Tir,
    decimal Vpl,
    bool RoteadaParaHumano,
    string? MotivoRoteamento,
    IReadOnlyList<AnoProjecao> Projecao);
