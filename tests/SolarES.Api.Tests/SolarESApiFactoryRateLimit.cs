using Microsoft.AspNetCore.Hosting;

namespace SolarES.Api.Tests;

/// <summary>
/// Mesma base de SolarESApiFactory, mas com limites de rate limit baixos o suficiente
/// para estourar em poucas chamadas dentro de um teste, sem depender de esperar a
/// janela real passar.
/// </summary>
public sealed class SolarESApiFactoryRateLimit : SolarESApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("RateLimit:PublicoLeitura:Limite", "2");
        builder.UseSetting("RateLimit:PublicoLeitura:JanelaMinutos", "10");
        builder.UseSetting("RateLimit:PublicoSimulacao:Limite", "2");
        builder.UseSetting("RateLimit:PublicoSimulacao:JanelaMinutos", "10");
    }
}
