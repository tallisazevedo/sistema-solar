using System.Net;
using System.Net.Http.Json;
using SolarES.Api.Contratos;

namespace SolarES.Api.Tests;

public class AutenticacaoApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly HttpClient _cliente;

    public AutenticacaoApiTests(SolarESApiFactory factory)
    {
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task Get_SemHeaderDeAutenticacao_Retorna401()
    {
        var response = await _cliente.GetAsync("/api/catalogo/modulos-fotovoltaicos");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComSenhaErrada_Retorna401()
    {
        var response = await _cliente.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(SolarESApiFactory.DonoEmail, "senha-errada"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_RetornaTokenComPerfil()
    {
        var response = await _cliente.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(SolarESApiFactory.DonoEmail, SolarESApiFactory.DonoSenha));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var corpo = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(corpo!.Token));
    }

    [Fact]
    public async Task Post_AutenticadoComoVendedor_Retorna403()
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail, SolarESApiFactory.VendedorSenha);

        var response = await _cliente.PostAsJsonAsync(
            "/api/catalogo/modulos-fotovoltaicos",
            new ModuloFotovoltaicoRequest("F", "M", 550, 1134, 2278, 21.3m, false, true));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PublicarConfiguracao_AutenticadoComoVendedor_Retorna403()
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail, SolarESApiFactory.VendedorSenha);

        var response = await _cliente.PostAsync($"/api/configuracao/rascunhos/{Guid.NewGuid()}/publicar", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
