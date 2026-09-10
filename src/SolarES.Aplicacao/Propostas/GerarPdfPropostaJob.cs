using System.Text.Json;
using Hangfire;
using SolarES.Aplicacao.Configuracao;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Simulacao;

namespace SolarES.Aplicacao.Propostas;

/// <summary>
/// Job do Hangfire (T21) que gera o PDF da proposta em segundo plano -- o POST que
/// cria a Proposta so enfileira, nunca espera este metodo terminar (aceite: "geracao
/// nao bloqueia request"). Se qualquer etapa lancar, o metodo propaga a excecao: e
/// assim que o Hangfire sabe que o job falhou e aciona o retry automatico (aceite:
/// "job falho e reprocessavel").
/// </summary>
public sealed class GerarPdfPropostaJob(
    IPropostaRepository propostaRepositorio,
    ISimulacaoRepository simulacaoRepositorio,
    IConfiguracaoVersaoRepository configuracaoRepositorio,
    IGeradorPdfProposta geradorPdf,
    IArmazenamentoPdf armazenamento)
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecutarAsync(Guid propostaId, CancellationToken ct)
    {
        var proposta = await propostaRepositorio.ObterPorIdAsync(propostaId, ct)
            ?? throw new InvalidOperationException($"Proposta '{propostaId}' nao encontrada.");

        // Sempre a versao de configuracao gravada na proposta, nunca a ativa --
        // recalcular uma proposta antiga tem que reproduzir o numero original.
        var configuracaoVersao = await configuracaoRepositorio.ObterPorIdAsync(proposta.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versao de configuracao da proposta nao encontrada.");

        var simulacao = await simulacaoRepositorio.ObterPorIdAsync(proposta.SimulacaoId, ct)
            ?? throw new InvalidOperationException("Simulacao da proposta nao encontrada.");

        var resultado = JsonSerializer.Deserialize<ResultadoSimulacao>(simulacao.ResultadoSnapshot)
            ?? throw new InvalidOperationException("Nao foi possivel ler o resultado congelado da simulacao.");

        var pdf = geradorPdf.Gerar(proposta.Numero, proposta.ValidaAte, configuracaoVersao.Payload, resultado);
        var caminho = await armazenamento.SalvarAsync(proposta.Numero, pdf, ct);

        proposta.ArquivoPdfUrl = caminho;
        await propostaRepositorio.SalvarAlteracoesAsync(ct);
    }
}
