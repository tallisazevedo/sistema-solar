using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Proposta;

namespace SolarES.Infraestrutura.Envios;

/// <summary>
/// Adaptador SMTP real (T25). As credenciais (Smtp:Usuario, Smtp:Senha) vem só de
/// configuração fora do repositório -- variáveis de ambiente ou user-secrets em
/// produção, nunca de appsettings.json versionado.
/// </summary>
public sealed class CanalEnvioEmailSmtp(IConfiguration configuracao) : ICanalEnvioProposta
{
    public CanalEnvio Canal => CanalEnvio.Email;

    public async Task<string?> EnviarAsync(string destino, string numeroProposta, byte[] pdf, CancellationToken ct)
    {
        var host = configuracao["Smtp:Host"] ?? throw new InvalidOperationException("Configuração 'Smtp:Host' ausente.");
        var porta = int.TryParse(configuracao["Smtp:Porta"], out var valor) ? valor : 587;
        var remetente = configuracao["Smtp:Remetente"] ?? throw new InvalidOperationException("Configuração 'Smtp:Remetente' ausente.");
        var usuario = configuracao["Smtp:Usuario"];
        var senha = configuracao["Smtp:Senha"];

        using var cliente = new SmtpClient(host, porta) { EnableSsl = true };
        if (!string.IsNullOrEmpty(usuario))
        {
            cliente.Credentials = new NetworkCredential(usuario, senha);
        }

        using var mensagem = new MailMessage(remetente, destino)
        {
            Subject = $"Sua proposta SolarES {numeroProposta}",
            Body = "Segue em anexo a proposta de energia solar solicitada.",
        };
        using var anexo = new MemoryStream(pdf);
        mensagem.Attachments.Add(new Attachment(anexo, $"{numeroProposta}.pdf", "application/pdf"));

        await cliente.SendMailAsync(mensagem, ct);
        return null;
    }
}
