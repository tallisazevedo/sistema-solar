using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SolarES.Api.Contratos;

namespace SolarES.Api.Tests;

/// <summary>Issue #30: toda resposta de erro segue Problem Details, em portugues, com traceId.</summary>
public class ProblemDetailsApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly HttpClient _cliente;

    public ProblemDetailsApiTests(SolarESApiFactory factory)
    {
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ComCamposFaltando_Devolve400ComErrosPorCampoEmPortugues()
    {
        var resposta = await _cliente.PostAsJsonAsync("/api/auth/login", new { });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(corpo.TryGetProperty("traceId", out _));
        Assert.True(corpo.TryGetProperty("errors", out var erros));

        var chaves = erros.EnumerateObject().Select(propriedade => propriedade.Name).ToList();
        Assert.Contains("Email", chaves);
        Assert.Contains("Senha", chaves);

        var mensagemEmail = erros.GetProperty("Email").EnumerateArray().First().GetString();
        Assert.DoesNotContain("field", mensagemEmail, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Email", mensagemEmail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_Devolve401ComProblemDetailsETraceId()
    {
        var resposta = await _cliente.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest("naoexiste@teste.solares", "senha-errada"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(corpo.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task ExcecaoNaoMapeada_ForaDeDevelopment_Devolve500GenericoSemDetalheInterno()
    {
        await using var factoryNaoDevelopment = new SolarESApiFactoryNaoDevelopment();
        var cliente = factoryNaoDevelopment.CreateClient();

        var resposta = await cliente.GetAsync("/api/_diagnosticos/erro-nao-tratado");

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(corpo.TryGetProperty("traceId", out _));

        var detalhe = corpo.GetProperty("detail").GetString();
        Assert.DoesNotContain("NotSupportedException", detalhe, StringComparison.Ordinal);
        Assert.DoesNotContain("System.", detalhe, StringComparison.Ordinal);
        Assert.DoesNotContain(" at ", detalhe, StringComparison.Ordinal);
    }
}
