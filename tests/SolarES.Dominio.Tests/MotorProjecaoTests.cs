using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

public class MotorProjecaoTests
{
    private static readonly ConfiguracaoCalculo Configuracao = ConfiguracaoCalculoBaseline.Criar();

    // Tarifa e Fio B reais da EDP ES B1 (T07).
    private const decimal TarifaCheiaAno1 = 0.337590m + 0.348950m;
    private const decimal ValorFioBPorKwhAno1 = 0.209152m;

    private const int AnoCalendarioInicial = 2026;

    private static List<GeracaoMes> CriarGeracaoMensalAno1(decimal geracaoPorMes, decimal consumoCompensavelPorMes) =>
        Enumerable.Range(1, 12)
            .Select(mes => new GeracaoMes(mes, geracaoPorMes, consumoCompensavelPorMes, Math.Min(geracaoPorMes, consumoCompensavelPorMes)))
            .ToList();

    [Fact]
    public void Projetar_RetornaUmAnoParaCadaAnoDoHorizonteConfigurado()
    {
        var geracaoMensal = CriarGeracaoMensalAno1(500m, 470m);

        var resultado = MotorProjecao.Projetar(geracaoMensal, Configuracao, AnoCalendarioInicial, TarifaCheiaAno1, ValorFioBPorKwhAno1);

        Assert.Equal((int)Configuracao.HorizonteAnos.Valor, resultado.Count);
    }

    [Fact]
    public void Projetar_AnoCalendario_ComecaNoInicialEIncrementaUmPorAno()
    {
        var geracaoMensal = CriarGeracaoMensalAno1(500m, 470m);

        var resultado = MotorProjecao.Projetar(geracaoMensal, Configuracao, AnoCalendarioInicial, TarifaCheiaAno1, ValorFioBPorKwhAno1);

        for (var i = 0; i < resultado.Count; i++)
        {
            Assert.Equal(i + 1, resultado[i].Ano);
            Assert.Equal(AnoCalendarioInicial + i, resultado[i].AnoCalendario);
        }
    }

    [Fact]
    public void Projetar_GeracaoAnual_DecresceComADegradacao()
    {
        // Consumo compensavel bem acima da geracao: energia compensada e limitada
        // pela geracao (nao pelo consumo), isolando o efeito puro da degradacao.
        var geracaoMensal = CriarGeracaoMensalAno1(geracaoPorMes: 400m, consumoCompensavelPorMes: 10000m);

        var resultado = MotorProjecao.Projetar(geracaoMensal, Configuracao, AnoCalendarioInicial, TarifaCheiaAno1, ValorFioBPorKwhAno1);

        Assert.True(resultado[0].GeracaoAnualKwh > resultado[1].GeracaoAnualKwh);
        Assert.True(resultado[1].GeracaoAnualKwh > resultado[2].GeracaoAnualKwh);
    }

    [Fact]
    public void Projetar_EconomiaEmTermosReais_DecresceNosPrimeirosAnos()
    {
        // Geracao limitante (nao o consumo) -- isola degradacao + Fio B crescente,
        // os dois efeitos que devem derrubar a economia REAL no inicio do horizonte
        // (2026->2027->2028, quando o percentual do Fio B salta 60%->75%->90%).
        var geracaoMensal = CriarGeracaoMensalAno1(geracaoPorMes: 400m, consumoCompensavelPorMes: 10000m);

        var resultado = MotorProjecao.Projetar(geracaoMensal, Configuracao, AnoCalendarioInicial, TarifaCheiaAno1, ValorFioBPorKwhAno1);

        Assert.Equal(2026, resultado[0].AnoCalendario);
        Assert.Equal(2027, resultado[1].AnoCalendario);
        Assert.Equal(2028, resultado[2].AnoCalendario);

        Assert.True(resultado[0].EconomiaLiquidaAnualEmTermosReaisReais > resultado[1].EconomiaLiquidaAnualEmTermosReaisReais);
        Assert.True(resultado[1].EconomiaLiquidaAnualEmTermosReaisReais > resultado[2].EconomiaLiquidaAnualEmTermosReaisReais);
    }

    [Fact]
    public void Projetar_PercentualFioBUsado_BateComMotorFioB()
    {
        var geracaoMensal = CriarGeracaoMensalAno1(400m, 10000m);

        var resultado = MotorProjecao.Projetar(geracaoMensal, Configuracao, AnoCalendarioInicial, TarifaCheiaAno1, ValorFioBPorKwhAno1);

        // Reconstroi a economia liquida real do ano 1 usando MotorFioB diretamente
        // (percentual de 2026 = 60%) e confere que bate com o que a projecao produziu.
        var percentual2026 = MotorFioB.ResolverPercentualAno(Configuracao, 2026);
        var economiaLiquidaEsperadaAno1 = 12 * 400m * (TarifaCheiaAno1 - ValorFioBPorKwhAno1 * percentual2026);

        Assert.Equal(economiaLiquidaEsperadaAno1, resultado[0].EconomiaLiquidaAnualEmTermosReaisReais, 6);
    }
}
