using System.ComponentModel.DataAnnotations;
using SolarES.Dominio.Catalogo;

namespace SolarES.Api.Contratos;

public sealed record ModuloFotovoltaicoRequest(
    [Required, MaxLength(200)] string Fabricante,
    [Required, MaxLength(200)] string Modelo,
    [Range(1, int.MaxValue)] int PotenciaW,
    [Range(1, int.MaxValue)] int LarguraMm,
    [Range(1, int.MaxValue)] int AlturaMm,
    [Range(0, 100)] decimal EficienciaPercentual,
    bool ResistenteNevoaSalina,
    bool Ativo)
{
    public ModuloFotovoltaico ParaEntidade(Guid id) => new()
    {
        Id = id,
        Fabricante = Fabricante,
        Modelo = Modelo,
        PotenciaW = PotenciaW,
        LarguraMm = LarguraMm,
        AlturaMm = AlturaMm,
        EficienciaPercentual = EficienciaPercentual,
        ResistenteNevoaSalina = ResistenteNevoaSalina,
        Ativo = Ativo,
    };
}

public sealed record ModuloFotovoltaicoResponse(
    Guid Id,
    string Fabricante,
    string Modelo,
    int PotenciaW,
    int LarguraMm,
    int AlturaMm,
    decimal EficienciaPercentual,
    bool ResistenteNevoaSalina,
    bool Ativo)
{
    public static ModuloFotovoltaicoResponse DeEntidade(ModuloFotovoltaico entidade) => new(
        entidade.Id, entidade.Fabricante, entidade.Modelo, entidade.PotenciaW, entidade.LarguraMm,
        entidade.AlturaMm, entidade.EficienciaPercentual, entidade.ResistenteNevoaSalina, entidade.Ativo);
}
