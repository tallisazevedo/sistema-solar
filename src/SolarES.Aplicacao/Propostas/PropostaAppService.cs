using Hangfire;
using SolarES.Aplicacao.Configuracao;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Proposta;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Aplicacao.Propostas;

public sealed class PropostaAppService(
    IPropostaRepository propostaRepositorio,
    ISimulacaoRepository simulacaoRepositorio,
    IConfiguracaoVersaoRepository configuracaoRepositorio,
    IArmazenamentoPdf armazenamento,
    IBackgroundJobClient jobs,
    TimeProvider relogio)
{
    public async Task<PropostaEntidade> GerarAsync(Guid simulacaoId, CancellationToken ct)
    {
        var simulacao = await simulacaoRepositorio.ObterPorIdAsync(simulacaoId, ct)
            ?? throw new InvalidOperationException("Simulacao nao encontrada.");

        var configuracaoVersao = await configuracaoRepositorio.ObterPorIdAsync(simulacao.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versao de configuracao da simulacao nao encontrada.");

        var hoje = relogio.GetUtcNow();
        var sequencial = await propostaRepositorio.ContarAsync(ct) + 1;
        var validaAte = hoje.AddDays(configuracaoVersao.Payload.TextosProposta.Valor.ValidadeDias);

        var proposta = new PropostaEntidade
        {
            Id = Guid.NewGuid(),
            SimulacaoId = simulacao.Id,
            Numero = $"PROP-{hoje.Year}-{sequencial:D4}",
            ConfiguracaoVersaoId = configuracaoVersao.Id,
            ValidaAte = validaAte,
            Status = StatusProposta.Emitida,
            CriadoEm = hoje,
            AtualizadoEm = hoje,
        };

        propostaRepositorio.Adicionar(proposta);
        await propostaRepositorio.SalvarAlteracoesAsync(ct);

        // So enfileira -- o metodo devolve antes do PDF existir, entao o POST nunca
        // espera a geracao (aceite da T21: "geracao nao bloqueia request").
        jobs.Enqueue<GerarPdfPropostaJob>(job => job.ExecutarAsync(proposta.Id, CancellationToken.None));

        return proposta;
    }

    public async Task<(byte[] ConteudoPdf, string Numero)> ObterPdfAsync(Guid propostaId, CancellationToken ct)
    {
        var proposta = await propostaRepositorio.ObterPorIdAsync(propostaId, ct)
            ?? throw new InvalidOperationException("Proposta nao encontrada.");

        if (proposta.ArquivoPdfUrl is null)
        {
            throw new PropostaAindaNaoGeradaException(proposta.Numero);
        }

        var pdf = await armazenamento.LerAsync(proposta.ArquivoPdfUrl, ct)
            ?? throw new InvalidOperationException($"Arquivo do PDF da proposta '{proposta.Numero}' nao encontrado no armazenamento.");

        return (pdf, proposta.Numero);
    }
}
