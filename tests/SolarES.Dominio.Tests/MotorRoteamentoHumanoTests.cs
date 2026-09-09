using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

public class MotorRoteamentoHumanoTests
{
    private static readonly ConfiguracaoCalculo Configuracao = ConfiguracaoCalculoBaseline.Criar();

    private static EntradaSimulacao CriarEntrada(bool possuiGeracaoPropria) => new(
        Enumerable.Repeat(500m, 12).ToList(),
        TipoLigacao.Monofasica,
        Subgrupo.B1,
        "3205309",
        TipoTelhado.Ceramico,
        100m,
        possuiGeracaoPropria);

    [Fact]
    public void Avaliar_ComGeracaoPropriaExistente_Roteia()
    {
        var entrada = CriarEntrada(possuiGeracaoPropria: true);

        var (roteada, motivo) = MotorRoteamentoHumano.Avaliar(entrada, Configuracao, potenciaInstaladaKwp: 3m);

        Assert.True(roteada);
        Assert.Contains("gera", motivo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Avaliar_ComPotenciaAcimaDoLimiteConfigurado_Roteia()
    {
        var entrada = CriarEntrada(possuiGeracaoPropria: false);
        var limite = Configuracao.LimiteKwpRoteamentoHumano.Valor;

        var (roteada, motivo) = MotorRoteamentoHumano.Avaliar(entrada, Configuracao, potenciaInstaladaKwp: limite + 1m);

        Assert.True(roteada);
        Assert.Contains("limite", motivo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Avaliar_CasoNormal_NaoRoteia()
    {
        var entrada = CriarEntrada(possuiGeracaoPropria: false);
        var limite = Configuracao.LimiteKwpRoteamentoHumano.Valor;

        var (roteada, motivo) = MotorRoteamentoHumano.Avaliar(entrada, Configuracao, potenciaInstaladaKwp: limite - 1m);

        Assert.False(roteada);
        Assert.Null(motivo);
    }
}
