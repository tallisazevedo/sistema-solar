using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SolarES.Aplicacao.Propostas;

namespace SolarES.Api.Controllers;

/// <summary>
/// Webhook da WhatsApp Cloud API (T25.2). Anonimo por natureza -- a Meta nao manda
/// bearer token, so' a assinatura HMAC do corpo. AllowAnonymous aqui e' seguro porque
/// a assinatura e' validada antes de qualquer efeito (ver AssinaturaValida).
/// DisableRateLimiting: a Meta manda de poucos IPs proprios -- limitar por IP aqui
/// derrubaria a confirmacao de entrega de todos os clientes de uma vez.
/// </summary>
[ApiController]
[AllowAnonymous]
[DisableRateLimiting]
[Route("api/webhooks/whatsapp")]
public sealed class WebhookWhatsAppController(IConfiguration configuracao, WebhookWhatsAppAppService servico) : ControllerBase
{
    [HttpGet]
    public IActionResult Verificar(
        [FromQuery(Name = "hub.mode")] string? modo,
        [FromQuery(Name = "hub.verify_token")] string? token,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        var tokenEsperado = configuracao["WhatsApp:VerifyToken"];
        if (modo == "subscribe" && !string.IsNullOrEmpty(tokenEsperado) && token == tokenEsperado
            && !string.IsNullOrEmpty(challenge))
        {
            return Content(challenge, "text/plain");
        }
        return Unauthorized();
    }

    [HttpPost]
    public async Task<IActionResult> Receber(CancellationToken ct)
    {
        using var leitor = new StreamReader(Request.Body, Encoding.UTF8);
        var corpo = await leitor.ReadToEndAsync(ct);

        if (!AssinaturaValida(corpo))
        {
            return Unauthorized();
        }

        await servico.ProcessarAsync(corpo, ct);
        return Ok();
    }

    private bool AssinaturaValida(string corpo)
    {
        var appSecret = configuracao["WhatsApp:AppSecret"];
        if (string.IsNullOrEmpty(appSecret)) return false;

        var assinaturaRecebida = Request.Headers["X-Hub-Signature-256"].ToString();
        if (string.IsNullOrEmpty(assinaturaRecebida)) return false;

        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(appSecret), Encoding.UTF8.GetBytes(corpo));
        var assinaturaEsperada = "sha256=" + Convert.ToHexStringLower(hash);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(assinaturaEsperada), Encoding.UTF8.GetBytes(assinaturaRecebida));
    }
}
