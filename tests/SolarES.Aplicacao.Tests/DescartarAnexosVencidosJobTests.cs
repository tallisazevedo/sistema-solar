using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SolarES.Aplicacao.Leads;
using SolarES.Dominio.Lead;
using SolarES.Infraestrutura.Anexos;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;
using LeadEntidade = SolarES.Dominio.Lead.Lead;

namespace SolarES.Aplicacao.Tests;

public class DescartarAnexosVencidosJobTests
{
    private sealed class AmbienteFalso(string diretorioRaiz) : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "SolarES.Aplicacao.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = diretorioRaiz;
        public string EnvironmentName { get; set; } = "Testing";
    }

    private static (DescartarAnexosVencidosJob Job, SolarESDbContext Contexto,
        ArmazenamentoAnexoContaEmDisco Armazenamento, DateTimeOffset Agora) CriarCenario()
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contexto = new SolarESDbContext(options);

        var diretorioRaiz = Directory.CreateTempSubdirectory("solares-anexos-teste-").FullName;
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection([new("AnexosConta:DiretorioArmazenamento", "App_Data/anexos-conta")])
            .Build();
        var armazenamento = new ArmazenamentoAnexoContaEmDisco(configuracao, new AmbienteFalso(diretorioRaiz));

        var agora = new DateTimeOffset(2026, 9, 11, 12, 0, 0, TimeSpan.Zero);
        var relogio = new FakeTimeProvider(agora);
        var repositorio = new EfLeadRepository(contexto);
        var job = new DescartarAnexosVencidosJob(repositorio, armazenamento, relogio);

        return (job, contexto, armazenamento, agora);
    }

    private static async Task<AnexoConta> AdicionarAnexoAsync(SolarESDbContext contexto,
        ArmazenamentoAnexoContaEmDisco armazenamento, DateTimeOffset recebidoEm, DateTimeOffset descartarAte)
    {
        var lead = LeadEntidade.Criar("Maria", "27999999999", "maria@exemplo.com", CanalPreferido.Email,
            Guid.NewGuid(), Guid.NewGuid(), [FinalidadeConsentimento.ContatoComercial, FinalidadeConsentimento.GuardaAnexoConta], recebidoEm);
        contexto.Leads.Add(lead);

        var caminho = await armazenamento.SalvarAsync(lead.Id, TipoAnexoConta.Pdf, "%PDF-teste"u8.ToArray(), CancellationToken.None);
        var anexo = AnexoConta.Criar(lead.Id, TipoAnexoConta.Pdf, 10, caminho, recebidoEm, descartarAte);
        contexto.AnexosConta.Add(anexo);
        await contexto.SaveChangesAsync();
        return anexo;
    }

    [Fact]
    public async Task ExecutarAsync_AnexoComPrazoVencido_ApagaArquivoEPreencheDescartadoEm()
    {
        var (job, contexto, armazenamento, agora) = CriarCenario();
        var anexo = await AdicionarAnexoAsync(contexto, armazenamento, agora.AddDays(-91), agora.AddDays(-1));

        await job.ExecutarAsync(CancellationToken.None);

        var atualizado = await contexto.AnexosConta.SingleAsync(a => a.Id == anexo.Id);
        Assert.Equal(agora, atualizado.DescartadoEm);
        Assert.Null(await armazenamento.LerAsync(anexo.CaminhoArmazenamento, CancellationToken.None));
    }

    [Fact]
    public async Task ExecutarAsync_AnexoComPrazoEmAberto_NaoApaga()
    {
        var (job, contexto, armazenamento, agora) = CriarCenario();
        var anexo = await AdicionarAnexoAsync(contexto, armazenamento, agora, agora.AddDays(90));

        await job.ExecutarAsync(CancellationToken.None);

        var atualizado = await contexto.AnexosConta.SingleAsync(a => a.Id == anexo.Id);
        Assert.Null(atualizado.DescartadoEm);
        Assert.NotNull(await armazenamento.LerAsync(anexo.CaminhoArmazenamento, CancellationToken.None));
    }
}
