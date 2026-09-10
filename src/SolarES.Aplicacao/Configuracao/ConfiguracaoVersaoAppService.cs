using SolarES.Dominio.Configuracao;

namespace SolarES.Aplicacao.Configuracao;

public sealed class ConfiguracaoVersaoAppService(IConfiguracaoVersaoRepository repositorio, TimeProvider relogio)
{
    public async Task<ConfiguracaoVersao> CriarRascunhoAsync(
        ConfiguracaoCalculo payload, string? observacao, CancellationToken ct)
    {
        if (await repositorio.ObterRascunhoAsync(ct) is not null)
        {
            throw new InvalidOperationException("Ja existe um rascunho ativo; publique ou descarte antes de criar outro.");
        }

        var numero = await repositorio.ObterProximoNumeroAsync(ct);
        var versao = ConfiguracaoVersao.CriarRascunho(numero, payload, observacao);
        repositorio.Adicionar(versao);
        await repositorio.SalvarAlteracoesAsync(ct);

        return versao;
    }

    public async Task PublicarAsync(Guid rascunhoId, Guid usuarioId, CancellationToken ct)
    {
        var rascunho = await repositorio.ObterPorIdAsync(rascunhoId, ct)
            ?? throw new InvalidOperationException("Rascunho nao encontrado.");

        var publicadaAtual = await repositorio.ObterPublicadaAtivaAsync(ct);

        rascunho.Publicar(usuarioId, relogio.GetUtcNow());
        publicadaAtual?.Arquivar();

        await repositorio.SalvarAlteracoesAsync(ct);
    }

    public Task<ConfiguracaoVersao?> ObterVersaoAtivaAsync(CancellationToken ct) =>
        repositorio.ObterPublicadaAtivaAsync(ct);
}
