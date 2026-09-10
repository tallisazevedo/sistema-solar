using System.Net;

namespace SolarES.Api.Tests;

public class OpenApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly HttpClient _cliente;

    public OpenApiTests(SolarESApiFactory factory)
    {
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task DocumentoOpenApi_EhGeradoEListaOsEndpointsDoAdmin()
    {
        var response = await _cliente.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("/api/catalogo/modulos-fotovoltaicos", corpo);
        Assert.Contains("/api/configuracao/rascunhos", corpo);
    }
}
