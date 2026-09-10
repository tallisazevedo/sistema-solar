using System.Net;
using System.Net.Http.Json;
using SolarES.Api.Contratos;

namespace SolarES.Api.Tests;

public class DistribuidorasApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly HttpClient _cliente;

    public DistribuidorasApiTests(SolarESApiFactory factory)
    {
        _cliente = factory.CreateClient();
    }

    private static DistribuidoraRequest CriarRequestValido() => new("EDP ES", "EDP-ES", Ativa: true);

    [Fact]
    public async Task CrudCompleto_CriarListarObterAtualizarRemover()
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.DonoEmail, SolarESApiFactory.DonoSenha);

        var criarResponse = await _cliente.PostAsJsonAsync("/api/tarifas/distribuidoras", CriarRequestValido());
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var criado = await criarResponse.Content.ReadFromJsonAsync<DistribuidoraResponse>();
        Assert.NotNull(criado);

        var listaResponse = await _cliente.GetAsync("/api/tarifas/distribuidoras");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<DistribuidoraResponse>>();
        Assert.Contains(lista!, d => d.Id == criado!.Id);

        var requestAtualizado = CriarRequestValido() with { Nome = "EDP Espirito Santo" };
        var atualizarResponse = await _cliente.PutAsJsonAsync($"/api/tarifas/distribuidoras/{criado!.Id}", requestAtualizado);
        Assert.Equal(HttpStatusCode.NoContent, atualizarResponse.StatusCode);

        var obterAposAtualizar = await _cliente.GetFromJsonAsync<DistribuidoraResponse>($"/api/tarifas/distribuidoras/{criado.Id}");
        Assert.Equal("EDP Espirito Santo", obterAposAtualizar!.Nome);

        var removerResponse = await _cliente.DeleteAsync($"/api/tarifas/distribuidoras/{criado.Id}");
        Assert.Equal(HttpStatusCode.NoContent, removerResponse.StatusCode);
    }

    [Fact]
    public async Task Post_AutenticadoComoVendedor_Retorna403()
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.VendedorEmail, SolarESApiFactory.VendedorSenha);

        var response = await _cliente.PostAsJsonAsync("/api/tarifas/distribuidoras", CriarRequestValido());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
