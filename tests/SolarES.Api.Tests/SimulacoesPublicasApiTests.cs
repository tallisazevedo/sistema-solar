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

public sealed class SimulacoesPublicasApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly SolarESApiFactory _factory;
    private readonly HttpClient _cliente;

    public SimulacoesPublicasApiTests(SolarESApiFactory factory)
    {
        _factory = factory;
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task Dado_VisitanteSemAutenticacao_Quando_CriaEReabreSimulacao_Entao_RetornaMesmoResultado()
    {
        const string codigoIbge = "8888888";
        await PrepararCenarioAsync(codigoIbge);
        _cliente.DefaultRequestHeaders.Authorization = null;

        var municipios = await _cliente.GetFromJsonAsync<List<MunicipioPublicoResponse>>("/api/publico/municipios");
        Assert.Contains(municipios!, municipio => municipio.CodigoIbge == codigoIbge && municipio.Nome == "Municipio Publico");

        var request = new CriarSimulacaoPublicaRequest(
            500m, TipoLigacao.Monofasica, PerfilImovel.Residencial, codigoIbge,
            TipoTelhado.Ceramico, 1000m, PossuiGeracaoPropria: false);
        var criar = await _cliente.PostAsJsonAsync("/api/publico/simulacoes", request);

        Assert.Equal(HttpStatusCode.Created, criar.StatusCode);
        var resultado = await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();
        Assert.NotNull(resultado);
        Assert.True(resultado!.PotenciaKwp > 0);
        Assert.True(resultado.CalibracaoPendente);

        var reaberta = await _cliente.GetFromJsonAsync<SimulacaoPublicaResponse>($"/api/publico/simulacoes/{resultado.Id}");
        Assert.Equal(resultado, reaberta);

        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var persistida = await contexto.Simulacoes.SingleAsync(s => s.Id == resultado.Id);
        var versaoPublicada = await contexto.ConfiguracoesVersao.SingleAsync(
            c => c.Status == StatusConfiguracaoVersao.Publicada);
        Assert.Equal(OrigemSimulacao.Landing, persistida.Origem);
        Assert.Equal(versaoPublicada.Id, persistida.ConfiguracaoVersaoId);
        using var entradas = JsonDocument.Parse(persistida.EntradasSnapshot);
        var historico = entradas.RootElement.GetProperty("HistoricoConsumoKwh");
        Assert.Equal(12, historico.GetArrayLength());
        Assert.All(historico.EnumerateArray(), consumo => Assert.Equal(500m, consumo.GetDecimal()));
        Assert.False(string.IsNullOrWhiteSpace(persistida.ResultadoSnapshot));
    }

    [Fact]
    public async Task Dada_SimulacaoInterna_Quando_ConsultadaNaSuperficiePublica_Entao_RetornaNaoEncontrada()
    {
        const string codigoIbge = "7777777";
        await PrepararCenarioAsync(codigoIbge);
        var interna = await _cliente.PostAsJsonAsync("/api/simulacoes", new EntradaSimulacaoRequest(
            Enumerable.Repeat(500m, 12).ToList(), TipoLigacao.Monofasica, Subgrupo.B1,
            codigoIbge, TipoTelhado.Ceramico, 1000m, false));
        var detalhe = await interna.Content.ReadFromJsonAsync<SimulacaoDetalheResponse>();
        _cliente.DefaultRequestHeaders.Authorization = null;

        var response = await _cliente.GetAsync($"/api/publico/simulacoes/{detalhe!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Dadas_OrigensDiferentes_Quando_PreflightPublico_Entao_LiberaSomenteAConfigurada()
    {
        using var permitida = new HttpRequestMessage(HttpMethod.Options, "/api/publico/municipios");
        permitida.Headers.Add("Origin", "https://landing.solares.test");
        permitida.Headers.Add("Access-Control-Request-Method", "GET");
        var respostaPermitida = await _cliente.SendAsync(permitida);

        Assert.Equal("https://landing.solares.test", respostaPermitida.Headers.GetValues("Access-Control-Allow-Origin").Single());

        using var negada = new HttpRequestMessage(HttpMethod.Options, "/api/publico/municipios");
        negada.Headers.Add("Origin", "https://nao-permitida.test");
        negada.Headers.Add("Access-Control-Request-Method", "GET");
        var respostaNegada = await _cliente.SendAsync(negada);

        Assert.False(respostaNegada.Headers.Contains("Access-Control-Allow-Origin"));

        using var administrativa = new HttpRequestMessage(HttpMethod.Options, "/api/simulacoes");
        administrativa.Headers.Add("Origin", "https://landing.solares.test");
        administrativa.Headers.Add("Access-Control-Request-Method", "GET");
        var respostaAdministrativa = await _cliente.SendAsync(administrativa);

        Assert.False(respostaAdministrativa.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private async Task PrepararCenarioAsync(string codigoIbge)
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(
            _cliente, SolarESApiFactory.DonoEmail, SolarESApiFactory.DonoSenha);
        var sufixo = Guid.NewGuid().ToString("N")[..8];
        await _cliente.PostAsJsonAsync("/api/catalogo/modulos-fotovoltaicos", new ModuloFotovoltaicoRequest(
            "Fabricante", $"Modulo-{sufixo}", 550, 1134, 2278, 21.3m, false, true));
        await _cliente.PostAsJsonAsync("/api/catalogo/inversores", new InversorRequest(
            "Fabricante", $"Inversor-{sufixo}", 10000, 2, TipoInversor.String, true));
        var distribuidoraHttp = await _cliente.PostAsJsonAsync("/api/tarifas/distribuidoras",
            new DistribuidoraRequest("Distribuidora Publica", $"DP-{sufixo}", true));
        var distribuidora = await distribuidoraHttp.Content.ReadFromJsonAsync<DistribuidoraResponse>();
        await _cliente.PostAsJsonAsync("/api/tarifas/municipios", new MunicipioHspRequest(
            codigoIbge, "Municipio Publico", -20m, -40m, 200m, distribuidora!.Id,
            Enumerable.Repeat(5m, 12).ToList(), "Teste"));
        await _cliente.PostAsJsonAsync("/api/tarifas/vigentes", new TarifaVigenteRequest(
            distribuidora.Id, Subgrupo.B1, .35m, .40m, .21m, .17m, .0925m,
            DateTimeOffset.UtcNow.AddDays(-1), null, "Teste", "Teste"));
        await _cliente.PostAsJsonAsync("/api/precificacao/faixas-preco", new FaixaPrecoRequest(
            0m, 500m, 3.5m, "Residencial", false, DateTimeOffset.UtcNow.AddDays(-1)));

        using var escopo = _factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        if (!await contexto.ConfiguracoesVersao.AnyAsync(c => c.Status == StatusConfiguracaoVersao.Publicada))
        {
            var agora = DateTimeOffset.UtcNow;
            var versao = ConfiguracaoVersao.CriarRascunho(1, ConfiguracaoCalculoBaseline.Criar(), "Teste");
            versao.Publicar(Guid.NewGuid(), agora);
            contexto.ConfiguracoesVersao.Add(versao);
            await contexto.SaveChangesAsync();
        }
    }
}
