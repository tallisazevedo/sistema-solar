using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SolarES.Aplicacao.Leads;
using SolarES.Dominio.Lead;

namespace SolarES.Infraestrutura.Anexos;

public sealed class ArmazenamentoAnexoContaEmDisco(IConfiguration configuracao, IHostEnvironment ambiente)
    : IArmazenamentoAnexoConta
{
    private readonly string _diretorio = Path.Combine(ambiente.ContentRootPath,
        configuracao["AnexosConta:DiretorioArmazenamento"]
            ?? throw new InvalidOperationException("Configuração 'AnexosConta:DiretorioArmazenamento' ausente."));

    public async Task<string> SalvarAsync(Guid leadId, TipoAnexoConta tipo, byte[] conteudo, CancellationToken ct)
    {
        Directory.CreateDirectory(_diretorio);
        var extensao = tipo switch { TipoAnexoConta.Pdf => "pdf", TipoAnexoConta.Jpeg => "jpg", _ => "png" };
        var caminho = Path.Combine(_diretorio, $"{leadId:N}.{extensao}");
        await File.WriteAllBytesAsync(caminho, conteudo, ct);
        return caminho;
    }

    public async Task<byte[]?> LerAsync(string caminho, CancellationToken ct) =>
        File.Exists(caminho) ? await File.ReadAllBytesAsync(caminho, ct) : null;

    public Task ApagarAsync(string caminho, CancellationToken ct)
    {
        if (File.Exists(caminho)) File.Delete(caminho);
        return Task.CompletedTask;
    }
}
