using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Premissas;
using SolarES.Dominio.Simulacao;
using SolarES.Infraestrutura.Pdf;

namespace SolarES.Infraestrutura.Tests;

public class GeradorPdfPropostaTests
{
    static GeradorPdfPropostaTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    private static readonly ResultadoSimulacao Resultado = new(
        PotenciaInstaladaKwp: 4.4m,
        QuantidadeModulos: 8,
        AreaNecessariaM2: 20m,
        CoberturaPercentual: 100m,
        Capex: 18480m,
        EconomiaMensalAno1: 325.16m,
        PaybackMesesSimples: 53,
        PaybackMesesDescontado: 76,
        Tir: 0.27m,
        Vpl: 34004.94m,
        RoteadaParaHumano: false,
        MotivoRoteamento: null,
        Projecao: Enumerable.Range(1, 25)
            .Select(ano => new AnoProjecao(ano, 2025 + ano, 3000m - ano * 10m, 1200m - ano * 15m, 1200m - ano * 15m))
            .ToList());

    private static ConfiguracaoCalculo ConfiguracaoComPremissaProvisoria() => ConfiguracaoCalculoBaseline.Criar();

    private static ConfiguracaoCalculo ConfiguracaoSemPremissaProvisoria()
    {
        var baseline = ConfiguracaoCalculoBaseline.Criar();
        return baseline with
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
        };
    }

    private static Premissa<T> ComOrigemValidada<T>(Premissa<T> premissa) => new(premissa.Valor, OrigemPremissa.FontePublica);

    [Fact]
    public void Gerar_ProduzPdfIntegro()
    {
        var gerador = new GeradorPdfProposta();

        var bytes = gerador.Gerar("PROP-2026-0001", DateTimeOffset.UtcNow.AddDays(15), ConfiguracaoComPremissaProvisoria(), Resultado);

        Assert.True(bytes.Length > 0);
        Assert.Equal("%PDF"u8.ToArray(), bytes.Take(4).ToArray());
    }

    [Fact]
    public void Gerar_ComEmSemPremissaProvisoria_ProduzDocumentosDiferentes()
    {
        var gerador = new GeradorPdfProposta();
        var numero = "PROP-2026-0002";
        var validaAte = DateTimeOffset.UtcNow.AddDays(15);

        var comMarcaDagua = gerador.Gerar(numero, validaAte, ConfiguracaoComPremissaProvisoria(), Resultado);
        var semMarcaDagua = gerador.Gerar(numero, validaAte, ConfiguracaoSemPremissaProvisoria(), Resultado);

        Assert.True(comMarcaDagua.Length > 0);
        Assert.True(semMarcaDagua.Length > 0);
        Assert.NotEqual(comMarcaDagua, semMarcaDagua);
    }
}
