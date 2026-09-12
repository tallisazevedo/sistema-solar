using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarES.Api.Contratos;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;
using SolarES.Infraestrutura.Persistencia;

namespace SolarES.Api.Tests;

/// <summary>Issue #31: rate limit por IP na superficie publica e no login.</summary>
public class RateLimitApiTests
{
    [Fact]
    public async Task Excedido_Devolve429ComRetryAfterEProblemDetails()
    {
        await using var factory = new SolarESApiFactoryRateLimit();
        var cliente = factory.CreateClient();

        await cliente.GetAsync("/api/publico/municipios");
        await cliente.GetAsync("/api/publico/municipios");
        var resposta = await cliente.GetAsync("/api/publico/municipios");

        Assert.Equal(HttpStatusCode.TooManyRequests, resposta.StatusCode);
        Assert.True(resposta.Headers.RetryAfter is not null);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(corpo.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task ComPoliticaPublicaSaturada_EndpointAutenticadoDoAdminContinuaRespondendo()
    {
        await using var factory = new SolarESApiFactoryRateLimit();
        var cliente = factory.CreateClient();

        await cliente.GetAsync("/api/publico/municipios");
        await cliente.GetAsync("/api/publico/municipios");
        var saturada = await cliente.GetAsync("/api/publico/municipios");
        Assert.Equal(HttpStatusCode.TooManyRequests, saturada.StatusCode);

        await SolarESApiFactory.ClienteAutenticadoAsync(cliente, SolarESApiFactory.DonoEmail, SolarESApiFactory.DonoSenha);
        var respostaAdmin = await cliente.GetAsync("/api/tarifas/distribuidoras");

        Assert.Equal(HttpStatusCode.OK, respostaAdmin.StatusCode);
    }

    [Fact]
    public async Task PoliticaPublicaSimulacaoSaturada_NaoPersisteSimulacaoAMais()
    {
        await using var factory = new SolarESApiFactoryRateLimit();
        var cliente = factory.CreateClient();
        await SolarESApiFactory.ClienteAutenticadoAsync(cliente, SolarESApiFactory.DonoEmail, SolarESApiFactory.DonoSenha);

        await cliente.PostAsJsonAsync("/api/catalogo/modulos-fotovoltaicos", new ModuloFotovoltaicoRequest(
            "Fabricante Teste", "Modelo Teste", PotenciaW: 550, LarguraMm: 1134, AlturaMm: 2278,
            EficienciaPercentual: 21.3m, ResistenteNevoaSalina: false, Ativo: true));
        await cliente.PostAsJsonAsync("/api/catalogo/inversores", new InversorRequest(
            "Fabricante Teste", "Inversor Teste", PotenciaW: 10000, QuantidadeMppt: 2, Tipo: TipoInversor.String, Ativo: true));
        var distribuidoraResponse = await cliente.PostAsJsonAsync(
            "/api/tarifas/distribuidoras", new DistribuidoraRequest("Distribuidora Teste", "DIST-RL", Ativa: true));
        var distribuidora = await distribuidoraResponse.Content.ReadFromJsonAsync<DistribuidoraResponse>();

        const string codigoIbge = "9999998";
        await cliente.PostAsJsonAsync("/api/tarifas/municipios", new MunicipioHspRequest(
            codigoIbge, "Municipio Teste", Latitude: -20.0m, Longitude: -40.0m, DistanciaMarKm: 200m,
            DistribuidoraId: distribuidora!.Id, HspPorMes: Enumerable.Repeat(5.0m, 12).ToList(), Fonte: "Teste"));
        await cliente.PostAsJsonAsync("/api/tarifas/vigentes", new TarifaVigenteRequest(
            DistribuidoraId: distribuidora.Id, Subgrupo: Subgrupo.B1, TarifaTe: 0.35m, TarifaTusd: 0.40m,
            ValorFioBPorKwh: 0.21m, AliquotaIcms: 0.17m, AliquotaPisCofins: 0.0925m,
            VigenciaInicio: DateTimeOffset.UtcNow.AddDays(-30), VigenciaFim: null,
            ResolucaoHomologatoria: "Teste", Fonte: "Teste"));
        await cliente.PostAsJsonAsync("/api/precificacao/faixas-preco", new FaixaPrecoRequest(
            KwpMinimo: 0m, KwpMaximo: 500m, PrecoPorWp: 3.5m, TipoInstalacao: "Residencial",
            KitLitoral: false, Vigencia: DateTimeOffset.UtcNow.AddDays(-30)));
        var rascunhoResponse = await cliente.PostAsJsonAsync(
            "/api/configuracao/rascunhos", new CriarRascunhoRequest(ConfiguracaoCalculoBaseline.Criar(), "Rascunho de teste."));
        var rascunho = await rascunhoResponse.Content.ReadFromJsonAsync<ConfiguracaoVersaoResponse>();
        await cliente.PostAsync($"/api/configuracao/rascunhos/{rascunho!.Id}/publicar", null);

        var pedido = new CriarSimulacaoPublicaRequest(
            ConsumoMedioMensalKwh: 500m, HistoricoConsumoKwh: null, TipoLigacao.Monofasica,
            PerfilImovel.Residencial, codigoIbge, TipoTelhado.Ceramico, AreaDisponivelM2: 30m,
            PossuiGeracaoPropria: false, SessaoFunilId: null);

        await cliente.PostAsJsonAsync("/api/publico/simulacoes", pedido);
        await cliente.PostAsJsonAsync("/api/publico/simulacoes", pedido);
        var terceira = await cliente.PostAsJsonAsync("/api/publico/simulacoes", pedido);
        Assert.Equal(HttpStatusCode.TooManyRequests, terceira.StatusCode);

        using var escopo = factory.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var totalSimulacoes = await banco.Simulacoes.CountAsync();
        Assert.Equal(2, totalSimulacoes);
    }
}
