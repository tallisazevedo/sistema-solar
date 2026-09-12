using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SolarES.Aplicacao.Propostas;

namespace SolarES.Infraestrutura.Envios;

public sealed class NotificadorInternoEmailEmDisco(IConfiguration configuracao, IHostEnvironment ambiente)
    : INotificadorInterno
{
    private readonly string _diretorio = Path.Combine(ambiente.ContentRootPath,
        configuracao["Email:DiretorioArmazenamento"]
            ?? throw new InvalidOperationException("Configuração 'Email:DiretorioArmazenamento' ausente."));

    public async Task EnviarEmailAsync(IReadOnlyCollection<string> destinatarios, string assunto, string corpo,
        CancellationToken ct)
    {
        Directory.CreateDirectory(_diretorio);
        var caminho = Path.Combine(_diretorio, $"{Guid.NewGuid():N}-notificacao-interna.txt");
        await File.WriteAllTextAsync(caminho,
            $"Para: {string.Join(", ", destinatarios)}\nAssunto: {assunto}\n\n{corpo}\n", ct);
    }
}
