using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SolarES.Aplicacao.Propostas;

namespace SolarES.Infraestrutura.Pdf;

/// <summary>
/// Grava o PDF gerado em disco local (T21). Armazenamento em nuvem e decisao de
/// deploy futura, fora do MVP local -- mesma nota ja registrada no plano da T20.
/// </summary>
public sealed class ArmazenamentoPdfEmDisco(IConfiguration configuracao, IHostEnvironment ambiente) : IArmazenamentoPdf
{
    private readonly string _diretorio = Path.Combine(
        ambiente.ContentRootPath,
        configuracao["Pdfs:DiretorioArmazenamento"]
            ?? throw new InvalidOperationException("Configuracao 'Pdfs:DiretorioArmazenamento' ausente."));

    public async Task<string> SalvarAsync(string numeroProposta, byte[] conteudo, CancellationToken ct)
    {
        Directory.CreateDirectory(_diretorio);
        var caminho = Path.Combine(_diretorio, $"{numeroProposta}.pdf");
        await File.WriteAllBytesAsync(caminho, conteudo, ct);
        return caminho;
    }

    public async Task<byte[]?> LerAsync(string caminho, CancellationToken ct) =>
        File.Exists(caminho) ? await File.ReadAllBytesAsync(caminho, ct) : null;
}
