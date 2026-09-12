using Microsoft.AspNetCore.Hosting;

namespace SolarES.Api.Tests;

/// <summary>Mesma base de SolarESApiFactory, com limite de envios por destino baixo o suficiente pra estourar em duas capturas.</summary>
public sealed class SolarESApiFactoryLimiteEnvios : SolarESApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("LimiteEnvios:PorDestino", "1");
        builder.UseSetting("LimiteEnvios:JanelaHoras", "24");
    }
}
