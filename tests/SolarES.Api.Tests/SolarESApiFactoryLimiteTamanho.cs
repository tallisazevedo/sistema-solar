using Microsoft.AspNetCore.Hosting;

namespace SolarES.Api.Tests;

/// <summary>
/// Mesma base de SolarESApiFactory, com limites de tamanho de corpo/anexo baixos o
/// suficiente para estourar com um payload pequeno dentro de um teste.
/// </summary>
public sealed class SolarESApiFactoryLimiteTamanho : SolarESApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Requisicoes:LimiteCorpoJsonBytes", "100");
    }
}

/// <summary>Mesma ideia, mas so' apertando o limite do anexo -- deixa o corpo JSON/multipart normal.</summary>
public sealed class SolarESApiFactoryLimiteAnexo : SolarESApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("AnexosConta:TamanhoMaximoBytes", "1000");
    }
}
