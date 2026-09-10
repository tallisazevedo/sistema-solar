using SolarES.Dominio.Configuracao;

namespace SolarES.Aplicacao.Configuracao;

public interface IConfiguracaoVersaoRepository
{
    Task<ConfiguracaoVersao?> ObterRascunhoAsync(CancellationToken ct);
    Task<ConfiguracaoVersao?> ObterPublicadaAtivaAsync(CancellationToken ct);
    Task<ConfiguracaoVersao?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<int> ObterProximoNumeroAsync(CancellationToken ct);
    void Adicionar(ConfiguracaoVersao versao);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
