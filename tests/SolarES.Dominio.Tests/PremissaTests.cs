using SolarES.Dominio.Premissas;

namespace SolarES.Dominio.Tests;

public class PremissaTests
{
    [Fact]
    public void Construir_ComOrigemProvisorioSemJustificativa_Lanca()
    {
        Assert.Throws<ArgumentException>(() => new Premissa<decimal>(0.78m, OrigemPremissa.Provisorio));
    }

    [Fact]
    public void Construir_ComOrigemProvisorioEJustificativaEmBranco_Lanca()
    {
        Assert.Throws<ArgumentException>(() => new Premissa<decimal>(0.78m, OrigemPremissa.Provisorio, "   "));
    }

    [Fact]
    public void Construir_ComOrigemLeiSemJustificativa_NaoLanca()
    {
        var premissa = new Premissa<decimal>(30m, OrigemPremissa.Lei);

        Assert.Equal(30m, premissa.Valor);
        Assert.Null(premissa.Justificativa);
    }

    [Fact]
    public void Construir_ComOrigemProvisorioEJustificativaPreenchida_NaoLanca()
    {
        var premissa = new Premissa<decimal>(0.78m, OrigemPremissa.Provisorio, "Assumido até validar com a empresa.");

        Assert.Equal(0.78m, premissa.Valor);
        Assert.Equal(OrigemPremissa.Provisorio, premissa.Origem);
    }
}
