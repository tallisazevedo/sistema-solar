using Microsoft.AspNetCore.Hosting;

namespace SolarES.Api.Tests;

/// <summary>
/// Mesma base de SolarESApiFactory, mas fora de "Development" -- usada para verificar que
/// o 500 generico do FallbackExceptionHandler nunca vaza detalhe interno em producao.
/// </summary>
public sealed class SolarESApiFactoryNaoDevelopment : SolarESApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseEnvironment("Testing");

        // appsettings.Development.json (unico com Jwt:Segredo) so carrega em Development.
        builder.UseSetting("Jwt:Segredo", "chave-de-teste-nao-development-32-bytes-minimo");
    }
}
