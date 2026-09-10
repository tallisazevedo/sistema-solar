using System.Text.Json;
using SolarES.Aplicacao.Configuracao;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Proposta;
using SolarES.Dominio.Simulacao;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Aplicacao.Propostas;

public sealed class PropostaAppService(
    IPropostaRepository propostaRepositorio,
    ISimulacaoRepository simulacaoRepositorio,
    IConfiguracaoVersaoRepository configuracaoRepositorio,
    IGeradorPdfProposta geradorPdf,
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

        return proposta;
    }

    public async Task<(byte[] ConteudoPdf, string Numero)> ObterPdfAsync(Guid propostaId, CancellationToken ct)
    {
        var proposta = await propostaRepositorio.ObterPorIdAsync(propostaId, ct)
            ?? throw new InvalidOperationException("Proposta nao encontrada.");

        // Sempre a versao de configuracao gravada na proposta, nunca a ativa --
        // recalcular uma proposta antiga tem que reproduzir o numero original.
        var configuracaoVersao = await configuracaoRepositorio.ObterPorIdAsync(proposta.ConfiguracaoVersaoId, ct)
            ?? throw new InvalidOperationException("Versao de configuracao da proposta nao encontrada.");

        var simulacao = await simulacaoRepositorio.ObterPorIdAsync(proposta.SimulacaoId, ct)
            ?? throw new InvalidOperationException("Simulacao da proposta nao encontrada.");

        var resultado = JsonSerializer.Deserialize<ResultadoSimulacao>(simulacao.ResultadoSnapshot)
            ?? throw new InvalidOperationException("Nao foi possivel ler o resultado congelado da simulacao.");

        var pdf = geradorPdf.Gerar(proposta.Numero, proposta.ValidaAte, configuracaoVersao.Payload, resultado);
        return (pdf, proposta.Numero);
    }
}
