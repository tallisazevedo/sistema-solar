using System.Reflection;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Premissas;

namespace SolarES.Dominio.Tests;

public class ConfiguracaoCalculoBaselineTests
{
    [Fact]
    public void Criar_NaoLanca()
    {
        var configuracao = ConfiguracaoCalculoBaseline.Criar();

        Assert.NotNull(configuracao);
    }

    [Fact]
    public void TodaPropriedade_DeConfiguracaoCalculo_EhPremissa()
    {
        var propriedadesQueNaoSaoPremissa = typeof(ConfiguracaoCalculo)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(propriedade => !EhTipoPremissa(propriedade.PropertyType))
            .Select(propriedade => propriedade.Name)
            .ToList();

        Assert.Empty(propriedadesQueNaoSaoPremissa);
    }

    [Fact]
    public void CronogramaFioB_TemOsSeisAnosDoArtigo27()
    {
        var configuracao = ConfiguracaoCalculoBaseline.Criar();

        var cronograma = configuracao.CronogramaFioB.Valor
            .ToDictionary(item => item.Ano, item => item.Percentual);

        Assert.Equal(6, cronograma.Count);
        Assert.Equal(0.15m, cronograma[2023]);
        Assert.Equal(0.30m, cronograma[2024]);
        Assert.Equal(0.45m, cronograma[2025]);
        Assert.Equal(0.60m, cronograma[2026]);
        Assert.Equal(0.75m, cronograma[2027]);
        Assert.Equal(0.90m, cronograma[2028]);
        Assert.Equal(OrigemPremissa.Lei, configuracao.CronogramaFioB.Origem);
    }

    [Fact]
    public void KitLitoral_TemQuatorzeMunicipiosComCodigoIbgeDeSeteDigitos()
    {
        var configuracao = ConfiguracaoCalculoBaseline.Criar();

        var codigos = configuracao.KitLitoral.Valor.MunicipiosCodigoIbge;

        Assert.Equal(14, codigos.Count);
        Assert.All(codigos, codigo => Assert.Matches("^\\d{7}$", codigo));
    }

    private static bool EhTipoPremissa(Type tipo) =>
        tipo.IsGenericType && tipo.GetGenericTypeDefinition() == typeof(Premissa<>);
}
