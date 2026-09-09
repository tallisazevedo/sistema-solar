namespace SolarES.Dominio.Simulacao;

/// <summary>Mes vai de 1 (janeiro) a 12 (dezembro).</summary>
public sealed record GeracaoMes(
    int Mes,
    decimal GeracaoKwh,
    decimal ConsumoCompensavelKwh,
    decimal EnergiaCompensadaKwh);
