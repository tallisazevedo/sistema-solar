using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SolarES.Infraestrutura.Envios;

namespace SolarES.Infraestrutura.Tests;

public class CanalEnvioWhatsAppTests
{
    private sealed class HandlerFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public List<(string Caminho, string? Corpo)> RequisicoesRecebidas { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var corpo = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
            RequisicoesRecebidas.Add((request.RequestUri!.AbsolutePath, corpo));
            return responder(request);
        }
    }

    private static CanalEnvioWhatsApp CriarCanal(HandlerFalso handler)
    {
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://graph.facebook.com/v21.0/") };
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new("WhatsApp:NumeroTelefoneId", "1234567890"),
                new("WhatsApp:TokenAcesso", "token-falso"),
                new("WhatsApp:NomeTemplate", "proposta_solar"),
                new("WhatsApp:IdiomaTemplate", "pt_BR"),
            ])
            .Build();
        return new CanalEnvioWhatsApp(http, configuracao);
    }

    [Fact]
    public async Task EnviarAsync_CaminhoFeliz_SobeMidiaEMandaTemplateComIdDaMidia()
    {
        var handler = new HandlerFalso(request =>
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/media", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"id":"midia-123"}""", Encoding.UTF8, "application/json"),
                };
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"messages":[{"id":"wamid.abc"}]}""", Encoding.UTF8, "application/json"),
            };
        });
        var canal = CriarCanal(handler);

        var idMensagem = await canal.EnviarAsync("27999999999", "PROP-2026-0001", "%PDF-teste"u8.ToArray(), CancellationToken.None);

        Assert.Equal("wamid.abc", idMensagem);
        Assert.Equal(2, handler.RequisicoesRecebidas.Count);
        Assert.EndsWith("/media", handler.RequisicoesRecebidas[0].Caminho, StringComparison.Ordinal);
        Assert.EndsWith("/messages", handler.RequisicoesRecebidas[1].Caminho, StringComparison.Ordinal);
        var corpoMensagem = JsonDocument.Parse(handler.RequisicoesRecebidas[1].Corpo!).RootElement;
        Assert.Equal("27999999999", corpoMensagem.GetProperty("to").GetString());
        Assert.Equal("proposta_solar", corpoMensagem.GetProperty("template").GetProperty("name").GetString());
        var parametroDocumento = corpoMensagem.GetProperty("template").GetProperty("components")[0]
            .GetProperty("parameters")[0].GetProperty("document");
        Assert.Equal("midia-123", parametroDocumento.GetProperty("id").GetString());
    }

    [Fact]
    public async Task EnviarAsync_UploadDeMidiaFalha_Lanca()
    {
        var handler = new HandlerFalso(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
        var canal = CriarCanal(handler);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => canal.EnviarAsync("27999999999", "PROP-2026-0001", "%PDF-teste"u8.ToArray(), CancellationToken.None));
    }
}
