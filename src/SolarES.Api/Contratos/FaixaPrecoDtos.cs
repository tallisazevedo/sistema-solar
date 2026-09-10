using System.ComponentModel.DataAnnotations;
using SolarES.Dominio.Precificacao;

namespace SolarES.Api.Contratos;

public sealed record FaixaPrecoRequest(
    [Range(0, double.MaxValue)] decimal KwpMinimo,
    [Range(0, double.MaxValue)] decimal KwpMaximo,
    [Range(0.01, double.MaxValue)] decimal PrecoPorWp,
    [Required, MaxLength(100)] string TipoInstalacao,
    bool KitLitoral,
    DateTimeOffset Vigencia)
{
    public FaixaPreco ParaEntidade(Guid id) => new()
    {
        Id = id,
        KwpMinimo = KwpMinimo,
        KwpMaximo = KwpMaximo,
        PrecoPorWp = PrecoPorWp,
        TipoInstalacao = TipoInstalacao,
        KitLitoral = KitLitoral,
        Vigencia = Vigencia,
    };
}

public sealed record FaixaPrecoResponse(
    Guid Id,
    decimal KwpMinimo,
    decimal KwpMaximo,
    decimal PrecoPorWp,
    string TipoInstalacao,
    bool KitLitoral,
    DateTimeOffset Vigencia)
{
    public static FaixaPrecoResponse DeEntidade(FaixaPreco entidade) => new(
        entidade.Id, entidade.KwpMinimo, entidade.KwpMaximo, entidade.PrecoPorWp,
        entidade.TipoInstalacao, entidade.KitLitoral, entidade.Vigencia);
}
