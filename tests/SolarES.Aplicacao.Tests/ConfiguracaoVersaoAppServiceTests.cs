using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Configuracao;
using SolarES.Dominio.Configuracao;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;

namespace SolarES.Aplicacao.Tests;

public class ConfiguracaoVersaoAppServiceTests
{
    private static (ConfiguracaoVersaoAppService Servico, SolarESDbContext Contexto) CriarServico(FakeTimeProvider relogio)
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var contexto = new SolarESDbContext(options);
        var repositorio = new ConfiguracaoVersaoRepository(contexto);
        var servico = new ConfiguracaoVersaoAppService(repositorio, relogio);

        return (servico, contexto);
    }

    [Fact]
    public async Task CriarRascunhoAsync_QuandoJaExisteRascunho_Lanca()
    {
        var (servico, _) = CriarServico(new FakeTimeProvider(DateTimeOffset.UtcNow));
        var payload = ConfiguracaoCalculoBaseline.Criar();

        await servico.CriarRascunhoAsync(payload, null, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => servico.CriarRascunhoAsync(payload, null, CancellationToken.None));
    }

    [Fact]
    public async Task PublicarAsync_ArquivaAPublicadaAnterior()
    {
        var (servico, contexto) = CriarServico(new FakeTimeProvider(DateTimeOffset.UtcNow));
        var payload = ConfiguracaoCalculoBaseline.Criar();

        var primeiraVersao = await servico.CriarRascunhoAsync(payload, null, CancellationToken.None);
        await servico.PublicarAsync(primeiraVersao.Id, Guid.NewGuid(), CancellationToken.None);

        var segundaVersao = await servico.CriarRascunhoAsync(payload, null, CancellationToken.None);
        await servico.PublicarAsync(segundaVersao.Id, Guid.NewGuid(), CancellationToken.None);

        var publicadas = await contexto.ConfiguracoesVersao
            .Where(c => c.Status == StatusConfiguracaoVersao.Publicada)
            .ToListAsync();
        var arquivadas = await contexto.ConfiguracoesVersao
            .Where(c => c.Status == StatusConfiguracaoVersao.Arquivada)
            .ToListAsync();

        Assert.Single(publicadas);
        Assert.Equal(segundaVersao.Id, publicadas[0].Id);
        Assert.Single(arquivadas);
        Assert.Equal(primeiraVersao.Id, arquivadas[0].Id);
        Assert.Equal(primeiraVersao.Numero + 1, segundaVersao.Numero);
    }

    [Fact]
    public async Task ObterVersaoAtivaAsync_SemPublicacao_RetornaNulo()
    {
        var (servico, _) = CriarServico(new FakeTimeProvider(DateTimeOffset.UtcNow));

        var ativa = await servico.ObterVersaoAtivaAsync(CancellationToken.None);

        Assert.Null(ativa);
    }

    [Fact]
    public async Task ObterVersaoAtivaAsync_AposPublicar_RetornaAVersaoPublicada()
    {
        var (servico, _) = CriarServico(new FakeTimeProvider(DateTimeOffset.UtcNow));
        var payload = ConfiguracaoCalculoBaseline.Criar();
        var versao = await servico.CriarRascunhoAsync(payload, null, CancellationToken.None);

        await servico.PublicarAsync(versao.Id, Guid.NewGuid(), CancellationToken.None);
        var ativa = await servico.ObterVersaoAtivaAsync(CancellationToken.None);

        Assert.NotNull(ativa);
        Assert.Equal(versao.Id, ativa!.Id);
    }
}
