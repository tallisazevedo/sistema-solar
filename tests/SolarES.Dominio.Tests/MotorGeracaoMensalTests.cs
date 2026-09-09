using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

public class MotorGeracaoMensalTests
{
    private static readonly ConfiguracaoCalculo Configuracao = ConfiguracaoCalculoBaseline.Criar();

    private static EntradaSimulacao CriarEntrada(IReadOnlyList<decimal> historicoConsumoKwh) => new(
        historicoConsumoKwh,
        TipoLigacao.Monofasica,
        Subgrupo.B1,
        "3205309",
        TipoTelhado.Ceramico,
        100m,
        false);

    [Fact]
    public void Calcular_ComHspVariandoPorMes_ProduzGeracaoDiferenteEntreMeses()
    {
        var entrada = CriarEntrada(Enumerable.Repeat(500m, 12).ToList());
        var hspSazonal = new List<decimal> { 6.5m, 6.2m, 5.5m, 4.6m, 3.9m, 3.6m, 3.7m, 4.4m, 4.8m, 5.2m, 5.7m, 6.3m };

        var resultado = MotorGeracaoMensal.Calcular(entrada, Configuracao, hspSazonal, potenciaInstaladaKwp: 5m);

        var geracoes = resultado.Select(m => m.GeracaoKwh).Distinct().ToList();
        Assert.True(geracoes.Count > 1);
        Assert.True(resultado[0].GeracaoKwh > resultado[5].GeracaoKwh);
    }

    [Fact]
    public void Calcular_ComHspConstante_ProduzGeracaoIgualEmTodosOsMeses()
    {
        var entrada = CriarEntrada(Enumerable.Repeat(500m, 12).ToList());
        var hspConstante = Enumerable.Repeat(5.0m, 12).ToList();

        var resultado = MotorGeracaoMensal.Calcular(entrada, Configuracao, hspConstante, potenciaInstaladaKwp: 5m);

        Assert.Single(resultado.Select(m => m.GeracaoKwh).Distinct());
    }

    [Fact]
    public void Calcular_EnergiaCompensada_EhOMinimoEntreGeracaoEConsumoCompensavel()
    {
        var entrada = CriarEntrada(Enumerable.Repeat(100m, 12).ToList());
        var hsp = Enumerable.Repeat(5.0m, 12).ToList();

        var resultado = MotorGeracaoMensal.Calcular(entrada, Configuracao, hsp, potenciaInstaladaKwp: 10m);

        Assert.All(resultado, mes =>
        {
            Assert.True(mes.EnergiaCompensadaKwh <= mes.GeracaoKwh);
            Assert.True(mes.EnergiaCompensadaKwh <= mes.ConsumoCompensavelKwh);
        });
    }

    [Fact]
    public void Calcular_ConsumoMensalAbaixoDoCustoDisponibilidade_ProduzZeroCompensavelNuncaNegativo()
    {
        var historico = Enumerable.Repeat(500m, 12).ToList();
        historico[3] = 10m;
        var entrada = CriarEntrada(historico);
        var hsp = Enumerable.Repeat(5.0m, 12).ToList();

        var resultado = MotorGeracaoMensal.Calcular(entrada, Configuracao, hsp, potenciaInstaladaKwp: 5m);

        Assert.Equal(0m, resultado[3].ConsumoCompensavelKwh);
        Assert.Equal(0m, resultado[3].EnergiaCompensadaKwh);
    }

    [Fact]
    public void Calcular_ComHspDeTamanhoDiferenteDeDoze_Lanca()
    {
        var entrada = CriarEntrada(Enumerable.Repeat(500m, 12).ToList());
        var hspIncompleto = Enumerable.Repeat(5.0m, 11).ToList();

        Assert.Throws<ArgumentException>(() =>
            MotorGeracaoMensal.Calcular(entrada, Configuracao, hspIncompleto, potenciaInstaladaKwp: 5m));
    }
}
