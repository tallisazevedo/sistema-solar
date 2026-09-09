using System.Diagnostics.CodeAnalysis;

namespace SolarES.Dominio.Catalogo;

public enum TipoInversor
{
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "'String inverter' e o termo padrao do setor fotovoltaico, distinto de microinversor.")]
    String,
    Micro,
}
