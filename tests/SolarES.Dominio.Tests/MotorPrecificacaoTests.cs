using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Precificacao;

namespace SolarES.Dominio.Tests;

public class MotorPrecificacaoTests
{
    private static FaixaPreco CriarFaixa(decimal kwpMinimo, decimal kwpMaximo, decimal precoPorWp, bool kitLitoral) => new()
    {
        Id = Guid.NewGuid(),
        KwpMinimo = kwpMinimo,
        KwpMaximo = kwpMaximo,
        PrecoPorWp = precoPorWp,
        TipoInstalacao = "Telhado",
        KitLitoral = kitLitoral,
        Vigencia = DateTimeOffset.UtcNow,
    };

    [Fact]
    public void CalcularCapex_ComKitLitoral_UsaAFaixaDePrecoProprio()
    {
        var catalogo = new List<FaixaPreco>
        {
            CriarFaixa(0m, 10m, precoPorWp: 4.50m, kitLitoral: false),
            CriarFaixa(0m, 10m, precoPorWp: 5.80m, kitLitoral: true),
        };

        var capex = MotorPrecificacao.CalcularCapex(potenciaInstaladaKwp: 5m, kitLitoral: true, catalogo);

        Assert.Equal(5m * 1000m * 5.80m, capex);
        Assert.NotEqual(5m * 1000m * 4.50m, capex);
    }

    [Fact]
    public void CalcularCapex_SemFaixaCompativel_RetornaNulo()
    {
        var catalogo = new List<FaixaPreco> { CriarFaixa(0m, 3m, precoPorWp: 4.50m, kitLitoral: false) };

        var capex = MotorPrecificacao.CalcularCapex(potenciaInstaladaKwp: 10m, kitLitoral: false, catalogo);

        Assert.Null(capex);
    }
}
