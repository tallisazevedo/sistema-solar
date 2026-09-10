using System.Net;
using System.Net.Http.Json;
using SolarES.Api.Contratos;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;

namespace SolarES.Api.Tests;

public class PropostasApiTests : IClassFixture<SolarESApiFactory>
{
    private readonly HttpClient _cliente;

    public PropostasApiTests(SolarESApiFactory factory)
    {
        _cliente = factory.CreateClient();
    }

    [Fact]
    public async Task FluxoCompleto_GerarProposta_DevolvePdfIntegroComValidaAte()
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

        const string codigoIbge = "9999999";
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
        var simulacaoResponse = await _cliente.PostAsJsonAsync("/api/simulacoes", entradaRequest);
        var simulacao = await simulacaoResponse.Content.ReadFromJsonAsync<SimulacaoDetalheResponse>();

        var gerarPropostaResponse = await _cliente.PostAsync($"/api/simulacoes/{simulacao!.Id}/proposta", null);
        Assert.Equal(HttpStatusCode.Created, gerarPropostaResponse.StatusCode);
        var proposta = await gerarPropostaResponse.Content.ReadFromJsonAsync<PropostaResponse>();
        Assert.NotNull(proposta);
        Assert.StartsWith("PROP-", proposta!.Numero, StringComparison.Ordinal);
        Assert.True(proposta.ValidaAte > DateTimeOffset.UtcNow);

        // T21: geracao e assincrona (job do Hangfire) -- o POST nao espera o PDF ficar
        // pronto, entao o teste espera o job rodar (Hangfire.InMemory processa em
        // background, mesmo raciocinio do "aguardarEGerar" do frontend).
        HttpResponseMessage pdfResponse;
        var tentativas = 0;
        do
        {
            pdfResponse = await _cliente.GetAsync($"/api/propostas/{proposta.Id}/pdf");
            if (pdfResponse.StatusCode == HttpStatusCode.OK)
            {
                break;
            }

            Assert.Equal(HttpStatusCode.NotFound, pdfResponse.StatusCode);
            await Task.Delay(200);
        } while (++tentativas < 25);

        Assert.Equal(HttpStatusCode.OK, pdfResponse.StatusCode);
        Assert.Equal("application/pdf", pdfResponse.Content.Headers.ContentType?.MediaType);
        var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
        Assert.True(pdfBytes.Length > 0);
        Assert.Equal("%PDF"u8.ToArray(), pdfBytes.Take(4).ToArray());
    }
}
