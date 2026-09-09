using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

public class MotorKitLitoralTests
{
    private static readonly ConfiguracaoCalculo Configuracao = ConfiguracaoCalculoBaseline.Criar();

    [Fact]
    public void Aplica_MunicipioDaListaConfigurada_RetornaVerdadeiroMesmoLonge()
    {
        var vitoria = "3205309"; // esta na lista do docs/02 / baseline (T03)

        var aplica = MotorKitLitoral.Aplica(Configuracao, vitoria, distanciaMarKm: 200m);

        Assert.True(aplica);
    }

    [Fact]
    public void Aplica_MunicipioForaDaListaMasDentroDoRaio_RetornaVerdadeiro()
    {
        var raioConfigurado = Configuracao.KitLitoral.Valor.RaioKm;

        var aplica = MotorKitLitoral.Aplica(Configuracao, "9999999", distanciaMarKm: raioConfigurado - 1m);

        Assert.True(aplica);
    }

    [Fact]
    public void Aplica_MunicipioForaDaListaEForaDoRaio_RetornaFalso()
    {
        var raioConfigurado = Configuracao.KitLitoral.Valor.RaioKm;

        var aplica = MotorKitLitoral.Aplica(Configuracao, "9999999", distanciaMarKm: raioConfigurado + 50m);

        Assert.False(aplica);
    }
}
