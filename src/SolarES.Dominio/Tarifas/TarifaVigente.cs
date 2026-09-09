using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tarifas;

public sealed class TarifaVigente : EntidadeBase
{
    public Guid DistribuidoraId { get; set; }
    public Subgrupo Subgrupo { get; set; }
    public decimal TarifaTe { get; set; }
    public decimal TarifaTusd { get; set; }
    public decimal ValorFioBPorKwh { get; set; }
    public decimal AliquotaIcms { get; set; }
    public decimal AliquotaPisCofins { get; set; }
    public DateTimeOffset VigenciaInicio { get; set; }
    public DateTimeOffset? VigenciaFim { get; set; }
    public required string ResolucaoHomologatoria { get; set; }
    public required string Fonte { get; set; }
}
