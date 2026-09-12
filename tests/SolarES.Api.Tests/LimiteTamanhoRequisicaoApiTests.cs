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

/// <summary>Issue #32: limites de tamanho de requisicao e de anexo.</summary>
public sealed class LimiteTamanhoRequisicaoApiTests
{
    [Fact]
    public async Task CorpoJsonAcimaDoLimite_Devolve413ComProblemDetails()
    {
        await using var factory = new SolarESApiFactoryLimiteTamanho();
        var cliente = factory.CreateClient();

        var corpoGrande = new StringContent(
            JsonSerializer.Serialize(new { tipo = 0, sessaoFunilId = Guid.NewGuid(), preenchimento = new string('x', 1000) }),
            System.Text.Encoding.UTF8, "application/json");

        var resposta = await cliente.PostAsync("/api/publico/eventos", corpoGrande);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
        var corpo = await resposta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(corpo.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task AnexoAcimaDoLimite_Devolve413ENaoPersisteLeadNemAnexo()
    {
        await using var factory = new SolarESApiFactoryLimiteAnexo();
        var cliente = factory.CreateClient();
        var simulacaoId = await PrepararSimulacaoAsync(factory, cliente, "6666666");

        var anexoGrande = new byte[2000]; // acima do limite de teste (1000 bytes)
        "%PDF-"u8.CopyTo(anexoGrande);

        var formulario = new MultipartFormDataContent
        {
            { new StringContent("Maria Silva"), "Nome" },
            { new StringContent("27999999999"), "Telefone" },
            { new StringContent("maria@exemplo.com"), "Email" },
            { new StringContent("Email"), "CanalPreferido" },
            { new StringContent("ContatoComercial"), "FinalidadesAceitas" },
            { new StringContent("contato-comercial-v1"), "VersaoTexto" },
        };
        var arquivo = new ByteArrayContent(anexoGrande);
        arquivo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        formulario.Add(arquivo, "Anexo", "conta.pdf");

        var resposta = await cliente.PostAsync($"/api/publico/simulacoes/{simulacaoId}/lead", formulario);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        using var escopo = factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        Assert.False(await contexto.Leads.AnyAsync(l => l.Email == "maria@exemplo.com"));
        Assert.Equal(0, await contexto.AnexosConta.CountAsync());
    }

    private static async Task<Guid> PrepararSimulacaoAsync(
        Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory, HttpClient cliente, string codigoIbge)
    {
        await SolarESApiFactory.ClienteAutenticadoAsync(cliente, SolarESApiFactory.DonoEmail, SolarESApiFactory.DonoSenha);
        var sufixo = Guid.NewGuid().ToString("N")[..8];
        await cliente.PostAsJsonAsync("/api/catalogo/modulos-fotovoltaicos", new ModuloFotovoltaicoRequest(
            "Fabricante", $"Modulo-{sufixo}", 550, 1134, 2278, 21.3m, false, true));
        await cliente.PostAsJsonAsync("/api/catalogo/inversores", new InversorRequest(
            "Fabricante", $"Inversor-{sufixo}", 10000, 2, TipoInversor.String, true));
        var distribuidoraHttp = await cliente.PostAsJsonAsync("/api/tarifas/distribuidoras",
            new DistribuidoraRequest("Distribuidora Publica", $"DP-{sufixo}", true));
        var distribuidora = await distribuidoraHttp.Content.ReadFromJsonAsync<DistribuidoraResponse>();
        await cliente.PostAsJsonAsync("/api/tarifas/municipios", new MunicipioHspRequest(
            codigoIbge, "Municipio Publico", -20m, -40m, 200m, distribuidora!.Id,
            Enumerable.Repeat(5m, 12).ToList(), "Teste"));
        await cliente.PostAsJsonAsync("/api/tarifas/vigentes", new TarifaVigenteRequest(
            distribuidora.Id, Subgrupo.B1, .35m, .40m, .21m, .17m, .0925m,
            DateTimeOffset.UtcNow.AddDays(-1), null, "Teste", "Teste"));
        await cliente.PostAsJsonAsync("/api/precificacao/faixas-preco", new FaixaPrecoRequest(
            0m, 500m, 3.5m, "Residencial", false, DateTimeOffset.UtcNow.AddDays(-1)));

        using var escopo = factory.Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        if (!await contexto.ConfiguracoesVersao.AnyAsync(c => c.Status == StatusConfiguracaoVersao.Publicada))
        {
            var agora = DateTimeOffset.UtcNow;
            var versao = ConfiguracaoVersao.CriarRascunho(1, ConfiguracaoCalculoBaseline.Criar(), "Teste");
            versao.Publicar(Guid.NewGuid(), agora);
            contexto.ConfiguracoesVersao.Add(versao);
            await contexto.SaveChangesAsync();
        }

        cliente.DefaultRequestHeaders.Authorization = null;
        var criar = await cliente.PostAsJsonAsync("/api/publico/simulacoes", new CriarSimulacaoPublicaRequest(
            500m, null, TipoLigacao.Monofasica, PerfilImovel.Residencial, codigoIbge,
            TipoTelhado.Ceramico, 1000m, PossuiGeracaoPropria: false, SessaoFunilId: null));
        var resultado = await criar.Content.ReadFromJsonAsync<SimulacaoPublicaResponse>();
        return resultado!.Id;
    }
}
