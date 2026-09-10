using System.Net;
using System.Net.Http.Json;
using SolarES.Api.Contratos;
using SolarES.Dominio.Configuracao;

namespace SolarES.Api.Tests;

public class ConfiguracaoVersaoApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly HttpClient _cliente;

    public ConfiguracaoVersaoApiTests(SolarESApiFactory factory)
    {
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task FluxoCompleto_CriarRascunho_NaoPermiteSegundo_PublicarExpoeComoAtiva()
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.DonoEmail, SolarESApiFactory.DonoSenha);

        var payload = ConfiguracaoCalculoBaseline.Criar();
        var criarRequest = new CriarRascunhoRequest(payload, "Rascunho inicial via API.");

        var criarResponse = await _cliente.PostAsJsonAsync("/api/configuracao/rascunhos", criarRequest);
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var rascunho = await criarResponse.Content.ReadFromJsonAsync<ConfiguracaoVersaoResponse>();
        Assert.NotNull(rascunho);
        Assert.Equal(StatusConfiguracaoVersao.Rascunho, rascunho!.Status);

        var segundoRascunhoResponse = await _cliente.PostAsJsonAsync("/api/configuracao/rascunhos", criarRequest);
        Assert.Equal(HttpStatusCode.BadRequest, segundoRascunhoResponse.StatusCode);

        var publicarResponse = await _cliente.PostAsync($"/api/configuracao/rascunhos/{rascunho.Id}/publicar", null);
        Assert.Equal(HttpStatusCode.NoContent, publicarResponse.StatusCode);

        var ativa = await _cliente.GetFromJsonAsync<ConfiguracaoVersaoResponse>("/api/configuracao/ativa");
        Assert.NotNull(ativa);
        Assert.Equal(rascunho.Id, ativa!.Id);
        Assert.Equal(StatusConfiguracaoVersao.Publicada, ativa.Status);
        Assert.NotNull(ativa.PublicadaPorUsuarioId);
    }
}
