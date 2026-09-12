using Hangfire;
using SolarES.Aplicacao.Configuracao;
using SolarES.Dominio;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Proposta;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Aplicacao.Propostas;

public sealed class EnvioPropostaAppService(
    IPropostaRepository propostaRepositorio,
    IConfiguracaoVersaoRepository configuracaoRepositorio,
    IEnvioPropostaRepository enviosRepositorio,
    IBackgroundJobClient jobs,
    TimeProvider relogio)
{
    public async Task<EnvioProposta> SolicitarEnvioAsync(Guid propostaId, CanalEnvio canal, string destino, CancellationToken ct)
    {
        var envio = await PrepararEnvioAsync(propostaId, canal, destino, ct);
        // So enfileira -- quem envia de fato e' o EnviarPropostaJob (retry do Hangfire).
        jobs.Enqueue<EnviarPropostaJob>(job => job.ExecutarAsync(envio.Id, CancellationToken.None));
        return envio;
    }

    /// <summary>
    /// Mesma guarda e criacao do EnvioProposta, mas sem enfileirar o envio -- quem
    /// precisa encadear como continuacao de outro job (T25.3: envio automatico,
    /// continuacao da geracao do PDF) decide quando e como disparar.
    /// </summary>
    public async Task<EnvioProposta> PrepararEnvioAsync(Guid propostaId, CanalEnvio canal, string destino, CancellationToken ct)
    {
        var proposta = await propostaRepositorio.ObterPorIdAsync(propostaId, ct)
            ?? throw new KeyNotFoundException("Proposta não encontrada.");
        await GarantirSemCalibracaoPendenteAsync(proposta, ct);

        var envio = EnvioProposta.Criar(proposta.Id, canal, destino, relogio.GetUtcNow());
        enviosRepositorio.Adicionar(envio);
        await enviosRepositorio.SalvarAlteracoesAsync(ct);
        return envio;
    }

    public Task<IReadOnlyList<EnvioProposta>> ListarEnviosAsync(Guid propostaId, CancellationToken ct) =>
        enviosRepositorio.ListarPorPropostaAsync(propostaId, ct);

    public async Task<bool> ReenviarAsync(Guid envioId, CancellationToken ct)
    {
        var envio = await enviosRepositorio.ObterPorIdAsync(envioId, ct);
        if (envio is null) return false;
        var proposta = await propostaRepositorio.ObterPorIdAsync(envio.PropostaId, ct)
            ?? throw new InvalidOperationException($"Proposta do envio '{envioId}' não encontrada.");
        await GarantirSemCalibracaoPendenteAsync(proposta, ct);

        jobs.Enqueue<EnviarPropostaJob>(job => job.ExecutarAsync(envio.Id, CancellationToken.None));
        return true;
    }

    /// <summary>Caminho obrigatorio pra qualquer canal: proposta sob premissa Provisoria nunca sai.</summary>
    private async Task GarantirSemCalibracaoPendenteAsync(PropostaEntidade proposta, CancellationToken ct)
    {
        var versao = await configuracaoRepositorio.ObterPorIdAsync(proposta.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versão de configuração da proposta não encontrada.");
        if (versao.Payload.PossuiPremissaProvisoria())
            throw new TransicaoInvalidaException(
                $"Proposta '{proposta.Numero}' está em calibração (premissa provisória) e não pode ser enviada ao cliente.");
    }
}
