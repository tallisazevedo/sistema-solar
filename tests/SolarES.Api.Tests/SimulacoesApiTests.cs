using System.Net;
using System.Net.Http.Json;
using SolarES.Api.Contratos;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;

namespace SolarES.Api.Tests;

public class SimulacoesApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly HttpClient _cliente;

    public SimulacoesApiTests(SolarESApiFactory factory)
    {
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task FluxoCompleto_CriarSimulacao_AparecerNaListagemEDetalhe()
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(_cliente, SolarESApiFactory.DonoEmail, SolarESApiFactory.DonoSenha);

        await _cliente.PostAsJsonAsync("/api/catalogo/modulos-fotovoltaicos", new ModuloFotovoltaicoRequest(
            "Fabricante Teste", "Modelo Teste", PotenciaW: 550, LarguraMm: 1134, AlturaMm: 2278,
            EficienciaPercentual: 21.3m, ResistenteNevoaSalina: false, Ativo: true));

        await _cliente.PostAsJsonAsync("/api/catalogo/inversores", new InversorRequest(
            "Fabricante Teste", "Inversor Teste", PotenciaW: 10000, QuantidadeMppt: 2, Tipo: TipoInversor.String, Ativo: true));

        var distribuidoraResponse = await _cliente.PostAsJsonAsync(
            "/api/tarifas/distribuidoras", new DistribuidoraRequest("Distribuidora Teste", "DIST-TESTE", Ativa: true));
        var distribuidora = await distribuidoraResponse.Content.ReadFromJsonAsync<DistribuidoraResponse>();

        const string codigoIbge = "9999999"; // fora da lista de kit litoral do baseline
        await _cliente.PostAsJsonAsync("/api/tarifas/municipios", new MunicipioHspRequest(
            codigoIbge, "Municipio Teste", Latitude: -20.0m, Longitude: -40.0m, DistanciaMarKm: 200m,
            DistribuidoraId: distribuidora!.Id, HspPorMes: Enumerable.Repeat(5.0m, 12).ToList(), Fonte: "Teste"));

        await _cliente.PostAsJsonAsync("/api/tarifas/vigentes", new TarifaVigenteRequest(
            DistribuidoraId: distribuidora.Id, Subgrupo: Subgrupo.B1, TarifaTe: 0.35m, TarifaTusd: 0.40m,
            ValorFioBPorKwh: 0.21m, AliquotaIcms: 0.17m, AliquotaPisCofins: 0.0925m,
            VigenciaInicio: DateTimeOffset.UtcNow.AddDays(-30), VigenciaFim: null,
            ResolucaoHomologatoria: "Teste", Fonte: "Teste"));

        await _cliente.PostAsJsonAsync("/api/precificacao/faixas-preco", new FaixaPrecoRequest(
            KwpMinimo: 0m, KwpMaximo: 500m, PrecoPorWp: 3.5m, TipoInstalacao: "Residencial",
            KitLitoral: false, Vigencia: DateTimeOffset.UtcNow.AddDays(-30)));

        var rascunhoResponse = await _cliente.PostAsJsonAsync(
            "/api/configuracao/rascunhos", new CriarRascunhoRequest(ConfiguracaoCalculoBaseline.Criar(), "Rascunho de teste."));
        var rascunho = await rascunhoResponse.Content.ReadFromJsonAsync<ConfiguracaoVersaoResponse>();
        await _cliente.PostAsync($"/api/configuracao/rascunhos/{rascunho!.Id}/publicar", null);

        var entradaRequest = new EntradaSimulacaoRequest(
            Enumerable.Repeat(500m, 12).ToList(), TipoLigacao.Monofasica, Subgrupo.B1, codigoIbge,
            TipoTelhado.Ceramico, AreaDisponivelM2: 1000m, PossuiGeracaoPropria: false);

        var criarResponse = await _cliente.PostAsJsonAsync("/api/simulacoes", entradaRequest);
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);
        var detalhe = await criarResponse.Content.ReadFromJsonAsync<SimulacaoDetalheResponse>();
        Assert.NotNull(detalhe);
        Assert.True(detalhe!.Resultado.PotenciaInstaladaKwp > 0);
        Assert.False(detalhe.Resultado.RoteadaParaHumano);

        var listaResponse = await _cliente.GetAsync("/api/simulacoes");
        var lista = await listaResponse.Content.ReadFromJsonAsync<List<SimulacaoResumoResponse>>();
        Assert.Contains(lista!, s => s.Id == detalhe.Id);

        var detalheNovamente = await _cliente.GetFromJsonAsync<SimulacaoDetalheResponse>($"/api/simulacoes/{detalhe.Id}");
        Assert.NotNull(detalheNovamente);
        Assert.Equal(detalhe.Resultado.PotenciaInstaladaKwp, detalheNovamente!.Resultado.PotenciaInstaladaKwp);
    }
}
