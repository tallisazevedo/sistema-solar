using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

public class MotorFinanceiroTests
{
    [Fact]
    public void Tir_CasoConhecidoDeUmPeriodo_RetornaDezPorCento()
    {
        // 1000 * 1,10^1 = 1100 -- TIR exata e analitica: 10%.
        var resultado = MotorFinanceiro.Calcular([1100m], capex: 1000m, taxaDesconto: 0.10m);

        Assert.NotNull(resultado.Tir);
        Assert.Equal(0.10m, resultado.Tir!.Value, 4);
    }

    [Fact]
    public void Tir_CasoConhecidoDeDoisPeriodos_RetornaDezPorCento()
    {
        // 1000 * 1,10^2 = 1210 -- confirma que o expoente por ano esta correto.
        var resultado = MotorFinanceiro.Calcular([0m, 1210m], capex: 1000m, taxaDesconto: 0.10m);

        Assert.NotNull(resultado.Tir);
        Assert.Equal(0.10m, resultado.Tir!.Value, 4);
    }

    [Fact]
    public void Vpl_NaTaxaIgualATir_EhZero()
    {
        var resultadoInicial = MotorFinanceiro.Calcular([1100m], capex: 1000m, taxaDesconto: 0.05m);

        // recalcula VPL usando a propria TIR encontrada como taxa de desconto
        var comTaxaIgualATir = MotorFinanceiro.Calcular([1100m], capex: 1000m, taxaDesconto: resultadoInicial.Tir!.Value);

        Assert.True(Math.Abs(comTaxaIgualATir.Vpl) < 0.01m);
    }

    [Fact]
    public void Vpl_ComTaxaDescontoZero_EhSomaDosFluxosMenosCapex()
    {
        var resultado = MotorFinanceiro.Calcular([300m, 300m, 300m], capex: 800m, taxaDesconto: 0m);

        Assert.Equal(100m, resultado.Vpl);
    }

    [Fact]
    public void PaybackSimples_CruzaCapexExatamenteNoFimDoAno_RetornaAnoEmMeses()
    {
        var fluxos = Enumerable.Repeat(400m, 5).ToList();

        var resultado = MotorFinanceiro.Calcular(fluxos, capex: 1200m, taxaDesconto: 0.08m);

        Assert.Equal(36, resultado.PaybackMesesSimples);
    }

    [Fact]
    public void PaybackDescontado_EhMaiorOuIgualAoSimples()
    {
        var fluxos = Enumerable.Repeat(400m, 5).ToList();

        var resultado = MotorFinanceiro.Calcular(fluxos, capex: 1200m, taxaDesconto: 0.08m);

        Assert.NotNull(resultado.PaybackMesesSimples);
        Assert.NotNull(resultado.PaybackMesesDescontado);
        Assert.True(resultado.PaybackMesesDescontado >= resultado.PaybackMesesSimples);
    }

    [Fact]
    public void Payback_QuandoNuncaAtingeOCapexNoHorizonte_RetornaNulo()
    {
        var fluxos = Enumerable.Repeat(10m, 5).ToList();

        var resultado = MotorFinanceiro.Calcular(fluxos, capex: 10000m, taxaDesconto: 0.08m);

        Assert.Null(resultado.PaybackMesesSimples);
        Assert.Null(resultado.PaybackMesesDescontado);
    }

    [Fact]
    public void Tir_QuandoNaoHaRaizNoIntervaloDeBusca_RetornaNulo()
    {
        // Fluxos anuais negativos (prejuizo continuo): o VPL fica negativo para
        // qualquer taxa, nao existe raiz real nenhuma -- nao e so fora do intervalo
        // de busca, e matematicamente ausente.
        var fluxos = Enumerable.Repeat(-100m, 3).ToList();

        var resultado = MotorFinanceiro.Calcular(fluxos, capex: 1000m, taxaDesconto: 0.08m);

        Assert.Null(resultado.Tir);
    }
}
