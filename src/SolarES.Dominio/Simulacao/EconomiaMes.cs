namespace SolarES.Dominio.Simulacao;

/// <summary>Mes vai de 1 (janeiro) a 12 (dezembro).</summary>
public sealed record EconomiaMes(
    int Mes,
    decimal CustoFioBReais,
    decimal EconomiaBrutaReais,
    decimal EconomiaLiquidaReais);
