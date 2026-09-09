namespace SolarES.Dominio.Simulacao;

public sealed record ResultadoCobertura(
    int QuantidadeModulosFinal,
    decimal PotenciaInstaladaFinalKwp,
    decimal CoberturaPercentual);
