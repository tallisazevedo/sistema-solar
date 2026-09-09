using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

public class MotorFioBTests
{
    private static readonly ConfiguracaoCalculo Configuracao = ConfiguracaoCalculoBaseline.Criar();

    [Theory]
    [InlineData(2023, 0.15)]
    [InlineData(2024, 0.30)]
    [InlineData(2025, 0.45)]
    [InlineData(2026, 0.60)]
    [InlineData(2027, 0.75)]
    [InlineData(2028, 0.90)]
    public void ResolverPercentualAno_AnoDoCronograma_RetornaOPercentualDaLei(int ano, double percentualEsperado)
    {
        var percentual = MotorFioB.ResolverPercentualAno(Configuracao, ano);

        Assert.Equal((decimal)percentualEsperado, percentual);
    }

    [Theory]
    [InlineData(2029)]
    [InlineData(2035)]
    [InlineData(2050)]
    public void ResolverPercentualAno_AnoDepoisDoCronograma_UsaFallback_NuncaAssume100(int ano)
    {
        var percentual = MotorFioB.ResolverPercentualAno(Configuracao, ano);

        Assert.Equal(0.90m, percentual);
        Assert.NotEqual(1.0m, percentual);
    }

    [Fact]
    public void ResolverPercentualAno_AnoAntesDoCronograma_UsaOPrimeiroConhecido()
    {
        var percentual = MotorFioB.ResolverPercentualAno(Configuracao, 2020);

        Assert.Equal(0.15m, percentual);
    }

    [Fact]
    public void CustoFioB_IncideSobreOComponenteFioB_NaoSobreATarifaCheia()
    {
        const decimal valorFioBPorKwh = 0.209152m;
        const decimal tarifaCheia = 0.68654m;
        const int ano = 2026;
        var geracaoMensal = new List<GeracaoMes> { new(1, GeracaoKwh: 600m, ConsumoCompensavelKwh: 470m, EnergiaCompensadaKwh: 470m) };

        var resultado = MotorFioB.Calcular(geracaoMensal, Configuracao, ano, valorFioBPorKwh, tarifaCheia);

        var percentual = MotorFioB.ResolverPercentualAno(Configuracao, ano);
        var custoFioBEsperado = 470m * valorFioBPorKwh * percentual;
        var custoSeIncidisseSobreTarifaCheia = 470m * tarifaCheia * percentual;

        Assert.Equal(custoFioBEsperado, resultado[0].CustoFioBReais);
        Assert.NotEqual(custoSeIncidisseSobreTarifaCheia, resultado[0].CustoFioBReais);
    }

    [Fact]
    public void Calcular_EconomiaLiquida_EhBrutaMenosCustoFioB()
    {
        var geracaoMensal = new List<GeracaoMes> { new(1, GeracaoKwh: 600m, ConsumoCompensavelKwh: 470m, EnergiaCompensadaKwh: 470m) };

        var resultado = MotorFioB.Calcular(geracaoMensal, Configuracao, 2026, 0.209152m, 0.68654m);

        Assert.Equal(resultado[0].EconomiaBrutaReais - resultado[0].CustoFioBReais, resultado[0].EconomiaLiquidaReais);
    }
}
