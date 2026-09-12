using Hangfire;
using SolarES.Aplicacao.Configuracao;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Configuracao;
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
    public async Task<PropostaEntidade> GerarAsync(Guid simulacaoId, Guid? responsavelUsuarioId, CancellationToken ct)
    {
        var (proposta, _) = await GerarComJobIdAsync(simulacaoId, responsavelUsuarioId, ct);
        return proposta;
    }

    /// <summary>
    /// Mesma geracao, mas devolve tambem o id do job de PDF enfileirado -- quem
    /// precisa encadear uma continuacao (T25.3: envio automatico) usa esse id.
    /// </summary>
    public async Task<(PropostaEntidade Proposta, string JobIdGeracaoPdf)> GerarComJobIdAsync(Guid simulacaoId,
        Guid? responsavelUsuarioId, CancellationToken ct)
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
            ResponsavelUsuarioId = responsavelUsuarioId,
            CriadoEm = hoje,
            AtualizadoEm = hoje,
        };

        propostaRepositorio.Adicionar(proposta);
        await propostaRepositorio.SalvarAlteracoesAsync(ct);

        // So enfileira -- o metodo devolve antes do PDF existir, entao o POST nunca
        // espera a geracao (aceite da T21: "geracao nao bloqueia request").
        var jobId = jobs.Enqueue<GerarPdfPropostaJob>(job => job.ExecutarAsync(proposta.Id, CancellationToken.None));

        return (proposta, jobId);
    }

    public Task<IReadOnlyList<PropostaEntidade>> ListarAsync(StatusProposta? status, CancellationToken ct) =>
        propostaRepositorio.ListarAsync(status, ct);

    public Task<PropostaEntidade?> ObterAsync(Guid id, CancellationToken ct) =>
        propostaRepositorio.ObterPorIdAsync(id, ct);

    public async Task<bool> PossuiCalibracaoPendenteAsync(PropostaEntidade proposta, CancellationToken ct)
    {
        var versao = await configuracaoRepositorio.ObterPorIdAsync(proposta.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versão de configuração da proposta não encontrada.");
        return versao.Payload.PossuiPremissaProvisoria();
    }

    public async Task<bool> AceitarAsync(Guid id, CancellationToken ct)
    {
        var proposta = await propostaRepositorio.ObterPorIdAsync(id, ct);
        if (proposta is null) return false;
        proposta.Aceitar(relogio.GetUtcNow());
        await propostaRepositorio.SalvarAlteracoesAsync(ct);
        return true;
    }

    public async Task<bool> MarcarPerdidaAsync(Guid id, string? motivo, CancellationToken ct)
    {
        var proposta = await propostaRepositorio.ObterPorIdAsync(id, ct);
        if (proposta is null) return false;
        proposta.MarcarPerdida(relogio.GetUtcNow(), motivo);
        await propostaRepositorio.SalvarAlteracoesAsync(ct);
        return true;
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
