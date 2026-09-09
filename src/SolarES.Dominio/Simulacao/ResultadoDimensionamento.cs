namespace SolarES.Dominio.Simulacao;

public sealed record ResultadoDimensionamento(
    decimal ConsumoCompensavel,
    decimal PotenciaNecessariaKwp,
    int QuantidadeModulos,
    decimal PotenciaInstaladaKwp,
    decimal AreaNecessariaM2,
    Guid ModuloId,
    Guid? InversorEscolhidoId);
