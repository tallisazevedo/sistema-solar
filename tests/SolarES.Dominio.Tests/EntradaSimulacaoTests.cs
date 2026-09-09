using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

public class EntradaSimulacaoTests
{
    [Fact]
    public void Construir_ComHistoricoDiferenteDeDozeMeses_Lanca()
    {
        var historicoCurto = Enumerable.Repeat(300m, 11).ToList();

        Assert.Throws<ArgumentException>(() => CriarEntrada(historicoCurto));
    }

    [Fact]
    public void ConsumoMedioMensal_CalculaMediaDosDozeMeses()
    {
        var historico = Enumerable.Range(1, 12).Select(mes => (decimal)mes * 100).ToList();

        var entrada = CriarEntrada(historico);

        Assert.Equal(650m, entrada.ConsumoMedioMensal);
    }

    private static EntradaSimulacao CriarEntrada(IReadOnlyList<decimal> historicoConsumoKwh) => new(
        historicoConsumoKwh,
        TipoLigacao.Monofasica,
        Subgrupo.B1,
        "3205200",
        TipoTelhado.Ceramico,
        30m);
}
