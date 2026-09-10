using System.ComponentModel.DataAnnotations;
using SolarES.Dominio.Catalogo;

namespace SolarES.Api.Contratos;

public sealed record InversorRequest(
    [Required, MaxLength(200)] string Fabricante,
    [Required, MaxLength(200)] string Modelo,
    [Range(1, int.MaxValue)] int PotenciaW,
    [Range(1, int.MaxValue)] int QuantidadeMppt,
    TipoInversor Tipo,
    bool Ativo)
{
    public Inversor ParaEntidade(Guid id) => new()
    {
        Id = id,
        Fabricante = Fabricante,
        Modelo = Modelo,
        PotenciaW = PotenciaW,
        QuantidadeMppt = QuantidadeMppt,
        Tipo = Tipo,
        Ativo = Ativo,
    };
}

public sealed record InversorResponse(
    Guid Id,
    string Fabricante,
    string Modelo,
    int PotenciaW,
    int QuantidadeMppt,
    TipoInversor Tipo,
    bool Ativo)
{
    public static InversorResponse DeEntidade(Inversor entidade) => new(
        entidade.Id, entidade.Fabricante, entidade.Modelo, entidade.PotenciaW,
        entidade.QuantidadeMppt, entidade.Tipo, entidade.Ativo);
}
