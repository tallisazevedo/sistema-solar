using Hangfire;

namespace SolarES.Aplicacao.Propostas;

/// <summary>
/// Job do Hangfire (T25) que efetivamente envia a proposta pelo canal escolhido. PDF
/// ainda nao gerado propaga excecao (igual ao GerarPdfPropostaJob) pra o Hangfire
/// retentar; falha no canal marca o EnvioProposta como Falhou e tambem propaga, pro
/// AutomaticRetry reprocessar.
/// </summary>
public sealed class EnviarPropostaJob(
    IEnvioPropostaRepository enviosRepositorio,
    IPropostaRepository propostaRepositorio,
    IArmazenamentoPdf armazenamentoPdf,
    IEnumerable<ICanalEnvioProposta> canais,
    ConfiguracaoLimiteEnvios limiteEnvios,
    TimeProvider relogio)
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecutarAsync(Guid envioId, CancellationToken ct)
    {
        var envio = await enviosRepositorio.ObterPorIdAsync(envioId, ct)
            ?? throw new InvalidOperationException($"Envio '{envioId}' não encontrado.");

        var proposta = await propostaRepositorio.ObterPorIdAsync(envio.PropostaId, ct)
            ?? throw new InvalidOperationException("Proposta do envio não encontrada.");

        if (proposta.ArquivoPdfUrl is null)
        {
            throw new PropostaAindaNaoGeradaException(proposta.Numero);
        }

        var pdf = await armazenamentoPdf.LerAsync(proposta.ArquivoPdfUrl, ct)
            ?? throw new InvalidOperationException($"Arquivo do PDF da proposta '{proposta.Numero}' não encontrado no armazenamento.");

        var canal = canais.SingleOrDefault(c => c.Canal == envio.Canal)
            ?? throw new InvalidOperationException($"Nenhum adaptador configurado para o canal '{envio.Canal}'.");

        var agora = relogio.GetUtcNow();

        // Anti-spam (issue #33): destino que ja recebeu o limite de envios na janela nao
        // chama o canal de novo -- sem isso, alguem poderia usar o formulario da landing
        // pra fazer a integradora mandar e-mail/WhatsApp em massa pra terceiros.
        var destinoNormalizado = NormalizarDestino(envio.Destino);
        var quantidadeRecente = await enviosRepositorio.ContarEnviadosPorDestinoDesdeAsync(
            destinoNormalizado, agora - limiteEnvios.Janela, ct);
        if (quantidadeRecente >= limiteEnvios.LimitePorDestino)
        {
            envio.MarcarFalhou(
                $"Limite de {limiteEnvios.LimitePorDestino} envios para este destino em {limiteEnvios.Janela.TotalHours}h foi atingido.",
                agora);
            await enviosRepositorio.SalvarAlteracoesAsync(ct);
            return; // Nao propaga excecao: nao ha' motivo pro Hangfire retentar antes da janela passar.
        }

        try
        {
            var idMensagem = await canal.EnviarAsync(envio.Destino, proposta.Numero, pdf, ct);
            envio.MarcarEnviado(idMensagem, agora);
            if (proposta.EnviadaEm is null)
            {
                proposta.EnviadaEm = agora;
                proposta.Canal = envio.Canal;
            }
            await enviosRepositorio.SalvarAlteracoesAsync(ct);
        }
        catch (Exception ex)
        {
            envio.MarcarFalhou(ex.Message, agora);
            await enviosRepositorio.SalvarAlteracoesAsync(ct);
            throw;
        }
    }

    private static string NormalizarDestino(string destino) => destino.Trim().ToLowerInvariant();
}
