using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Configuracao;
using SolarES.Dominio.Configuracao;

namespace SolarES.Infraestrutura.Persistencia.Repositorios;

public sealed class ConfiguracaoVersaoRepository(SolarESDbContext contexto) : IConfiguracaoVersaoRepository
{
    public Task<ConfiguracaoVersao?> ObterRascunhoAsync(CancellationToken ct) =>
        contexto.ConfiguracoesVersao.SingleOrDefaultAsync(c => c.Status == StatusConfiguracaoVersao.Rascunho, ct);

    public Task<ConfiguracaoVersao?> ObterPublicadaAtivaAsync(CancellationToken ct) =>
        contexto.ConfiguracoesVersao.SingleOrDefaultAsync(c => c.Status == StatusConfiguracaoVersao.Publicada, ct);

    public Task<ConfiguracaoVersao?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        contexto.ConfiguracoesVersao.SingleOrDefaultAsync(c => c.Id == id, ct);

    public void Adicionar(ConfiguracaoVersao versao) => contexto.ConfiguracoesVersao.Add(versao);

    public Task SalvarAlteracoesAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
