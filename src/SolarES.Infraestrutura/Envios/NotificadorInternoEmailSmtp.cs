using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using SolarES.Aplicacao.Propostas;

namespace SolarES.Infraestrutura.Envios;

public sealed class NotificadorInternoEmailSmtp(IConfiguration configuracao) : INotificadorInterno
{
    public async Task EnviarEmailAsync(IReadOnlyCollection<string> destinatarios, string assunto, string corpo,
        CancellationToken ct)
    {
        var host = configuracao["Smtp:Host"] ?? throw new InvalidOperationException("Configuração 'Smtp:Host' ausente.");
        var porta = int.TryParse(configuracao["Smtp:Porta"], out var valor) ? valor : 587;
        var remetente = configuracao["Smtp:Remetente"] ?? throw new InvalidOperationException("Configuração 'Smtp:Remetente' ausente.");
        var usuario = configuracao["Smtp:Usuario"];
        var senha = configuracao["Smtp:Senha"];

        using var cliente = new SmtpClient(host, porta) { EnableSsl = true };
        if (!string.IsNullOrEmpty(usuario)) cliente.Credentials = new NetworkCredential(usuario, senha);

        using var mensagem = new MailMessage { From = new MailAddress(remetente), Subject = assunto, Body = corpo };
        foreach (var destinatario in destinatarios) mensagem.To.Add(destinatario);
        await cliente.SendMailAsync(mensagem, ct);
    }
}
