using System.Text.Json;
using SolarES.Dominio.Proposta;

namespace SolarES.Aplicacao.Propostas;

/// <summary>
/// Traduz o payload de status do webhook da WhatsApp Cloud API (T25.2) pro
/// EnvioProposta correspondente. A validacao da assinatura X-Hub-Signature-256 e' do
/// controller -- aqui so' chega corpo ja autenticado.
/// </summary>
public sealed class WebhookWhatsAppAppService(IEnvioPropostaRepository enviosRepositorio, TimeProvider relogio)
{
    public async Task ProcessarAsync(string corpoJson, CancellationToken ct)
    {
        using var documento = JsonDocument.Parse(corpoJson);
        if (!documento.RootElement.TryGetProperty("entry", out var entradas)) return;

        foreach (var entrada in entradas.EnumerateArray())
        {
            if (!entrada.TryGetProperty("changes", out var mudancas)) continue;
            foreach (var mudanca in mudancas.EnumerateArray())
            {
                if (!mudanca.TryGetProperty("value", out var valor)) continue;
                if (!valor.TryGetProperty("statuses", out var statusList)) continue;
                foreach (var status in statusList.EnumerateArray())
                {
                    await ProcessarStatusAsync(status, ct);
                }
            }
        }
    }

    private async Task ProcessarStatusAsync(JsonElement status, CancellationToken ct)
    {
        if (!status.TryGetProperty("id", out var idElemento)) return;
        var idMensagem = idElemento.GetString();
        if (string.IsNullOrEmpty(idMensagem)) return;

        var envio = await enviosRepositorio.ObterPorIdMensagemProvedorAsync(idMensagem, ct);
        if (envio is null) return;

        var situacao = status.TryGetProperty("status", out var situacaoElemento) ? situacaoElemento.GetString() : null;
        var agora = relogio.GetUtcNow();
        switch (situacao)
        {
            case "delivered":
                envio.MarcarEntregue(agora);
                await enviosRepositorio.SalvarAlteracoesAsync(ct);
                break;
            case "failed":
                envio.MarcarFalhou(ExtrairErro(status), agora);
                await enviosRepositorio.SalvarAlteracoesAsync(ct);
                break;
        }
    }

    private static string ExtrairErro(JsonElement status)
    {
        if (status.TryGetProperty("errors", out var erros) && erros.GetArrayLength() > 0
            && erros[0].TryGetProperty("title", out var titulo) && titulo.GetString() is { Length: > 0 } texto)
        {
            return texto;
        }
        return "Falha reportada pelo WhatsApp.";
    }
}
