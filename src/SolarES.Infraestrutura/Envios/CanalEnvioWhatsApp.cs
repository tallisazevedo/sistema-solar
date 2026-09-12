using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Proposta;

namespace SolarES.Infraestrutura.Envios;

/// <summary>
/// Adaptador WhatsApp Cloud API (T25.2): sobe o PDF como midia e manda uma mensagem de
/// template aprovada referenciando essa midia. Nome e idioma do template vem de
/// configuracao -- o template em si precisa estar aprovado na conta Business (fora do
/// alcance deste codigo).
/// </summary>
public sealed class CanalEnvioWhatsApp(HttpClient http, IConfiguration configuracao) : ICanalEnvioProposta
{
    public CanalEnvio Canal => CanalEnvio.Whatsapp;

    public async Task<string?> EnviarAsync(string destino, string numeroProposta, byte[] pdf, CancellationToken ct)
    {
        var numeroTelefoneId = configuracao["WhatsApp:NumeroTelefoneId"]
            ?? throw new InvalidOperationException("Configuração 'WhatsApp:NumeroTelefoneId' ausente.");
        var tokenAcesso = configuracao["WhatsApp:TokenAcesso"]
            ?? throw new InvalidOperationException("Configuração 'WhatsApp:TokenAcesso' ausente.");
        var nomeTemplate = configuracao["WhatsApp:NomeTemplate"]
            ?? throw new InvalidOperationException("Configuração 'WhatsApp:NomeTemplate' ausente.");
        var idiomaTemplate = configuracao["WhatsApp:IdiomaTemplate"] ?? "pt_BR";

        var idMidia = await SubirMidiaAsync(numeroTelefoneId, tokenAcesso, numeroProposta, pdf, ct);
        return await EnviarTemplateAsync(numeroTelefoneId, tokenAcesso, nomeTemplate, idiomaTemplate,
            destino, numeroProposta, idMidia, ct);
    }

    private async Task<string> SubirMidiaAsync(string numeroTelefoneId, string tokenAcesso,
        string numeroProposta, byte[] pdf, CancellationToken ct)
    {
        using var conteudo = new MultipartFormDataContent();
        var arquivo = new ByteArrayContent(pdf);
        arquivo.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        conteudo.Add(arquivo, "file", $"{numeroProposta}.pdf");
        conteudo.Add(new StringContent("application/pdf"), "type");
        conteudo.Add(new StringContent("whatsapp"), "messaging_product");

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, $"{numeroTelefoneId}/media") { Content = conteudo };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenAcesso);
        using var resposta = await http.SendAsync(requisicao, ct);
        resposta.EnsureSuccessStatusCode();

        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("WhatsApp Cloud API não devolveu o id da mídia enviada.");
    }

    private async Task<string?> EnviarTemplateAsync(string numeroTelefoneId, string tokenAcesso,
        string nomeTemplate, string idiomaTemplate, string destino, string numeroProposta,
        string idMidia, CancellationToken ct)
    {
        var corpo = new
        {
            messaging_product = "whatsapp",
            to = destino,
            type = "template",
            template = new
            {
                name = nomeTemplate,
                language = new { code = idiomaTemplate },
                components = new object[]
                {
                    new
                    {
                        type = "header",
                        parameters = new object[]
                        {
                            new { type = "document", document = new { id = idMidia, filename = $"{numeroProposta}.pdf" } },
                        },
                    },
                },
            },
        };

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, $"{numeroTelefoneId}/messages")
        {
            Content = JsonContent.Create(corpo),
        };
        requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenAcesso);
        using var resposta = await http.SendAsync(requisicao, ct);
        resposta.EnsureSuccessStatusCode();

        var json = await resposta.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
        return json.GetProperty("messages")[0].GetProperty("id").GetString();
    }
}
