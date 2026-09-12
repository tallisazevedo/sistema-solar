using SolarES.Dominio.Metricas;

namespace SolarES.Dominio.Tests;

public sealed class EventoFunilTests
{
    [Fact]
    public void Inicio_ExigeSessaoValida()
    {
        Assert.Throws<ArgumentException>(() => EventoFunil.CriarInicio(Guid.Empty, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Conclusao_ExigeSimulacaoValida()
    {
        Assert.Throws<ArgumentException>(() =>
            EventoFunil.CriarConclusao(Guid.NewGuid(), Guid.Empty, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void LeadCapturado_ExigeSessaoValida()
    {
        Assert.Throws<ArgumentException>(() => EventoFunil.CriarLeadCapturado(Guid.Empty, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void AnexoOferecido_ExigeSessaoValida()
    {
        Assert.Throws<ArgumentException>(() => EventoFunil.CriarAnexoOferecido(Guid.Empty, DateTimeOffset.UtcNow));
    }
}
