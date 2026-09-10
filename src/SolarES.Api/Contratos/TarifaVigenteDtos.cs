using System.ComponentModel.DataAnnotations;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;

namespace SolarES.Api.Contratos;

public sealed record TarifaVigenteRequest(
    [Required] Guid DistribuidoraId,
    Subgrupo Subgrupo,
    [Range(0, double.MaxValue)] decimal TarifaTe,
    [Range(0, double.MaxValue)] decimal TarifaTusd,
    [Range(0, double.MaxValue)] decimal ValorFioBPorKwh,
    [Range(0, 1)] decimal AliquotaIcms,
    [Range(0, 1)] decimal AliquotaPisCofins,
    DateTimeOffset VigenciaInicio,
    DateTimeOffset? VigenciaFim,
    [Required, MaxLength(200)] string ResolucaoHomologatoria,
    [Required, MaxLength(500)] string Fonte)
{
    public TarifaVigente ParaEntidade(Guid id) => new()
    {
        Id = id,
        DistribuidoraId = DistribuidoraId,
        Subgrupo = Subgrupo,
        TarifaTe = TarifaTe,
        TarifaTusd = TarifaTusd,
        ValorFioBPorKwh = ValorFioBPorKwh,
        AliquotaIcms = AliquotaIcms,
        AliquotaPisCofins = AliquotaPisCofins,
        VigenciaInicio = VigenciaInicio,
        VigenciaFim = VigenciaFim,
        ResolucaoHomologatoria = ResolucaoHomologatoria,
        Fonte = Fonte,
    };
}

public sealed record TarifaVigenteResponse(
    Guid Id,
    Guid DistribuidoraId,
    Subgrupo Subgrupo,
    decimal TarifaTe,
    decimal TarifaTusd,
    decimal ValorFioBPorKwh,
    decimal AliquotaIcms,
    decimal AliquotaPisCofins,
    DateTimeOffset VigenciaInicio,
    DateTimeOffset? VigenciaFim,
    string ResolucaoHomologatoria,
    string Fonte)
{
    public static TarifaVigenteResponse DeEntidade(TarifaVigente entidade) => new(
        entidade.Id, entidade.DistribuidoraId, entidade.Subgrupo, entidade.TarifaTe, entidade.TarifaTusd,
        entidade.ValorFioBPorKwh, entidade.AliquotaIcms, entidade.AliquotaPisCofins,
        entidade.VigenciaInicio, entidade.VigenciaFim, entidade.ResolucaoHomologatoria, entidade.Fonte);
}
