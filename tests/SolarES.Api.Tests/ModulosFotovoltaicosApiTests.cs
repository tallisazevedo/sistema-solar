using System.Net;
using System.Net.Http.Json;
using SolarES.Api.Contratos;

namespace SolarES.Api.Tests;

public class ModulosFotovoltaicosApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly HttpClient _cliente;

    public ModulosFotovoltaicosApiTests(SolarESApiFactory factory)
    {
        _cliente = factory.CreateClient();
    }

    private static ModuloFotovoltaicoRequest CriarRequestValido() => new(
        "Fabricante Teste", "Modelo Teste", PotenciaW: 550, LarguraMm: 1134, AlturaMm: 2278,
        EficienciaPercentual: 21.3m, ResistenteNevoaSalina: false, Ativo: true);

    [Fact]
    public async Task CrudCompleto_CriarListarObterAtualizarRemover()
    {
        var criarResponse = await _cliente.PostAsJsonAsync("/api/catalogo/modulos-fotovoltaicos", CriarRequestValido());
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var criado = await criarResponse.Content.ReadFromJsonAsync<ModuloFotovoltaicoResponse>();
        Assert.NotNull(criado);

        var listaResponse = await _cliente.GetAsync("/api/catalogo/modulos-fotovoltaicos");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<ModuloFotovoltaicoResponse>>();
        Assert.Contains(lista!, m => m.Id == criado!.Id);

        var obterResponse = await _cliente.GetAsync($"/api/catalogo/modulos-fotovoltaicos/{criado!.Id}");
        Assert.Equal(HttpStatusCode.OK, obterResponse.StatusCode);

        var requestAtualizado = CriarRequestValido() with { PotenciaW = 600 };
        var atualizarResponse = await _cliente.PutAsJsonAsync($"/api/catalogo/modulos-fotovoltaicos/{criado.Id}", requestAtualizado);
        Assert.Equal(HttpStatusCode.NoContent, atualizarResponse.StatusCode);

        var obterAposAtualizar = await _cliente.GetFromJsonAsync<ModuloFotovoltaicoResponse>($"/api/catalogo/modulos-fotovoltaicos/{criado.Id}");
        Assert.Equal(600, obterAposAtualizar!.PotenciaW);

        var removerResponse = await _cliente.DeleteAsync($"/api/catalogo/modulos-fotovoltaicos/{criado.Id}");
        Assert.Equal(HttpStatusCode.NoContent, removerResponse.StatusCode);

        var obterAposRemover = await _cliente.GetAsync($"/api/catalogo/modulos-fotovoltaicos/{criado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, obterAposRemover.StatusCode);
    }

    [Fact]
    public async Task Criar_ComPotenciaNegativa_Retorna400()
    {
        var requestInvalido = CriarRequestValido() with { PotenciaW = -10 };

        var response = await _cliente.PostAsJsonAsync("/api/catalogo/modulos-fotovoltaicos", requestInvalido);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
