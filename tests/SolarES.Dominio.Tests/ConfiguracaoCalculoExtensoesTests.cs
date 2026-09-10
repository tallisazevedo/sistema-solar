using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Premissas;

namespace SolarES.Dominio.Tests;

public class ConfiguracaoCalculoExtensoesTests
{
    [Fact]
    public void PossuiPremissaProvisoria_ComBaseline_RetornaVerdadeiro()
    {
        // Baseline (T03) tem varias premissas Provisorio de proposito -- nada foi
        // validado com a empresa ainda.
        var configuracao = ConfiguracaoCalculoBaseline.Criar();

        Assert.True(configuracao.PossuiPremissaProvisoria());
    }

    [Fact]
    public void PossuiPremissaProvisoria_QuandoTodasAsPremissasSaoLeiOuFontePublica_RetornaFalso()
    {
        var baseline = ConfiguracaoCalculoBaseline.Criar();
        var configuracaoValidada = baseline with
        {
            PerformanceRatio = ComOrigemValidada(baseline.PerformanceRatio),
            DegradacaoAnual = ComOrigemValidada(baseline.DegradacaoAnual),
            InflacaoTarifaria = ComOrigemValidada(baseline.InflacaoTarifaria),
            TaxaDesconto = ComOrigemValidada(baseline.TaxaDesconto),
            HorizonteAnos = ComOrigemValidada(baseline.HorizonteAnos),
            OversizingMaximo = ComOrigemValidada(baseline.OversizingMaximo),
            FatorOrientacaoPadrao = ComOrigemValidada(baseline.FatorOrientacaoPadrao),
            EstrategiaFioBForaCronograma = ComOrigemValidada(baseline.EstrategiaFioBForaCronograma),
            LimiteKwpRoteamentoHumano = ComOrigemValidada(baseline.LimiteKwpRoteamentoHumano),
            KitLitoral = ComOrigemValidada(baseline.KitLitoral),
            TextosProposta = ComOrigemValidada(baseline.TextosProposta),
            // CronogramaFioB e CustoDisponibilidadePorLigacao ja sao Lei no baseline.
        };

        Assert.False(configuracaoValidada.PossuiPremissaProvisoria());
    }

    [Fact]
    public void PossuiPremissaProvisoria_ComUmaUnicaPremissaProvisoria_RetornaVerdadeiro()
    {
        var baseline = ConfiguracaoCalculoBaseline.Criar();
        var configuracaoQuaseValidada = baseline with
        {
            PerformanceRatio = ComOrigemValidada(baseline.PerformanceRatio),
            DegradacaoAnual = ComOrigemValidada(baseline.DegradacaoAnual),
            InflacaoTarifaria = ComOrigemValidada(baseline.InflacaoTarifaria),
            TaxaDesconto = ComOrigemValidada(baseline.TaxaDesconto),
            HorizonteAnos = ComOrigemValidada(baseline.HorizonteAnos),
            OversizingMaximo = ComOrigemValidada(baseline.OversizingMaximo),
            FatorOrientacaoPadrao = ComOrigemValidada(baseline.FatorOrientacaoPadrao),
            EstrategiaFioBForaCronograma = ComOrigemValidada(baseline.EstrategiaFioBForaCronograma),
            LimiteKwpRoteamentoHumano = ComOrigemValidada(baseline.LimiteKwpRoteamentoHumano),
            KitLitoral = ComOrigemValidada(baseline.KitLitoral),
            // TextosProposta permanece Provisorio de proposito.
        };

        Assert.True(configuracaoQuaseValidada.PossuiPremissaProvisoria());
    }

    private static Premissa<T> ComOrigemValidada<T>(Premissa<T> premissa) =>
        new(premissa.Valor, OrigemPremissa.FontePublica);
}
