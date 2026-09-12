using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SolarES.Dominio.Proposta;
using SolarES.Infraestrutura.Persistencia;

namespace SolarES.Api.Tests;

public sealed class WebhookWhatsAppApiTests : IClassFixture<SolarESApiFactory>
{
    private const string AppSecret = "segredo-de-teste-whatsapp";
    private const string VerifyToken = "verify-token-de-teste";

    private readonly WebApplicationFactory<Program> _factoryComSegredos;
    private readonly HttpClient _cliente;

    public WebhookWhatsAppApiTests(SolarESApiFactory factory)
    {
        _factoryComSegredos = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("WhatsApp:AppSecret", AppSecret);
            builder.UseSetting("WhatsApp:VerifyToken", VerifyToken);
        });
        _cliente = _factoryComSegredos.CreateClient();
    }

    [Fact]
    public async Task Dado_TokenDeVerificacaoCorreto_Quando_Handshake_Entao_DevolveOChallenge()
    {
        var resposta = await _cliente.GetAsync(
            $"/api/webhooks/whatsapp?hub.mode=subscribe&hub.verify_token={VerifyToken}&hub.challenge=desafio-123");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("desafio-123", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Dado_TokenDeVerificacaoIncorreto_Quando_Handshake_Entao_RetornaNaoAutorizado()
    {
        var resposta = await _cliente.GetAsync(
            "/api/webhooks/whatsapp?hub.mode=subscribe&hub.verify_token=token-errado&hub.challenge=desafio-123");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Dada_AssinaturaValida_Quando_RecebeStatusDelivered_Entao_AtualizaEnvioParaEntregue()
    {
        var envioId = await PrepararEnvioAsync();
        var corpo = PayloadDelivered("wamid.valido");

        var requisicao = CriarRequisicao(corpo, AssinaturaValida(corpo));
        var resposta = await _cliente.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        using var escopo = _factoryComSegredos.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var envio = await banco.EnviosProposta.SingleAsync(e => e.Id == envioId);
        Assert.Equal(StatusEnvioProposta.Entregue, envio.Status);
        Assert.NotNull(envio.EntregueEm);
    }

    [Fact]
    public async Task Dada_AssinaturaInvalida_Quando_RecebeStatus_Entao_RetornaNaoAutorizadoSemEfeito()
    {
        var envioId = await PrepararEnvioAsync();
        var corpo = PayloadDelivered("wamid.valido");

        var requisicao = CriarRequisicao(corpo, "sha256=assinatura-invalida");
        var resposta = await _cliente.SendAsync(requisicao);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        using var escopo = _factoryComSegredos.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var envio = await banco.EnviosProposta.SingleAsync(e => e.Id == envioId);
        Assert.Equal(StatusEnvioProposta.Enviado, envio.Status);
    }

    private async Task<Guid> PrepararEnvioAsync()
    {
        using var escopo = _factoryComSegredos.Services.CreateScope();
        var banco = escopo.ServiceProvider.GetRequiredService<SolarESDbContext>();
        var envio = EnvioProposta.Criar(Guid.NewGuid(), CanalEnvio.Whatsapp, "27999999999", DateTimeOffset.UtcNow);
        envio.MarcarEnviado("wamid.valido", DateTimeOffset.UtcNow);
        banco.EnviosProposta.Add(envio);
        await banco.SaveChangesAsync();
        return envio.Id;
    }

    private static HttpRequestMessage CriarRequisicao(string corpo, string assinatura)
    {
        var requisicao = new HttpRequestMessage(HttpMethod.Post, "/api/webhooks/whatsapp")
        {
            Content = new StringContent(corpo, Encoding.UTF8, "application/json"),
        };
        requisicao.Headers.Add("X-Hub-Signature-256", assinatura);
        return requisicao;
    }

    private static string AssinaturaValida(string corpo)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(AppSecret), Encoding.UTF8.GetBytes(corpo));
        return "sha256=" + Convert.ToHexStringLower(hash);
    }

    private static string PayloadDelivered(string idMensagem) =>
        "{\"entry\":[{\"changes\":[{\"value\":{\"statuses\":[{\"id\":\"" + idMensagem + "\",\"status\":\"delivered\"}]}}]}]}";
}
