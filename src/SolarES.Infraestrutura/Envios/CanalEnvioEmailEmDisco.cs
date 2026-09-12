using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Proposta;

namespace SolarES.Infraestrutura.Envios;

/// <summary>Adaptador de desenvolvimento (T25): grava a mensagem em disco em vez de enviar de verdade.</summary>
public sealed class CanalEnvioEmailEmDisco(IConfiguration configuracao, IHostEnvironment ambiente)
    : ICanalEnvioProposta
{
    private readonly string _diretorio = Path.Combine(ambiente.ContentRootPath,
        configuracao["Email:DiretorioArmazenamento"]
            ?? throw new InvalidOperationException("Configuração 'Email:DiretorioArmazenamento' ausente."));

    public CanalEnvio Canal => CanalEnvio.Email;

    public async Task<string?> EnviarAsync(string destino, string numeroProposta, byte[] pdf, CancellationToken ct)
    {
        Directory.CreateDirectory(_diretorio);
        var idMensagem = Guid.NewGuid().ToString("N");
        var caminhoPdf = Path.Combine(_diretorio, $"{idMensagem}-{numeroProposta}.pdf");
        await File.WriteAllBytesAsync(caminhoPdf, pdf, ct);
        var caminhoMetadados = Path.Combine(_diretorio, $"{idMensagem}.txt");
        await File.WriteAllTextAsync(caminhoMetadados,
            $"Para: {destino}\nProposta: {numeroProposta}\nAnexo: {Path.GetFileName(caminhoPdf)}\n", ct);
        return idMensagem;
    }
}
