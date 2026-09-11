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

public class ExpurgarLeadsInativosJobTests
{
    private sealed class AmbienteFalso(string diretorioRaiz) : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "SolarES.Aplicacao.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = diretorioRaiz;
        public string EnvironmentName { get; set; } = "Testing";
    }

    private static (ExpurgarLeadsInativosJob Job, SolarESDbContext Contexto,
        ArmazenamentoAnexoContaEmDisco Armazenamento, DateTimeOffset Agora) CriarCenario(int prazoMeses = 24)
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contexto = new SolarESDbContext(options);

        var diretorioRaiz = Directory.CreateTempSubdirectory("solares-expurgo-teste-").FullName;
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection([new("AnexosConta:DiretorioArmazenamento", "App_Data/anexos-conta")])
            .Build();
        var armazenamento = new ArmazenamentoAnexoContaEmDisco(configuracao, new AmbienteFalso(diretorioRaiz));

        var agora = new DateTimeOffset(2026, 9, 11, 12, 0, 0, TimeSpan.Zero);
        var relogio = new FakeTimeProvider(agora);
        var repositorio = new EfLeadRepository(contexto);
        var retencao = new ConfiguracaoRetencaoLgpd(90, prazoMeses);
        var job = new ExpurgarLeadsInativosJob(repositorio, armazenamento, retencao, relogio);

        return (job, contexto, armazenamento, agora);
    }

    private static async Task<LeadEntidade> AdicionarLeadComAnexoAsync(SolarESDbContext contexto,
        ArmazenamentoAnexoContaEmDisco armazenamento, DateTimeOffset criadoEm)
    {
        var lead = LeadEntidade.Criar("Maria", "27999999999", "maria@exemplo.com", CanalPreferido.Email,
            Guid.NewGuid(), Guid.NewGuid(), [FinalidadeConsentimento.ContatoComercial, FinalidadeConsentimento.GuardaAnexoConta], criadoEm);
        contexto.Leads.Add(lead);

        var caminho = await armazenamento.SalvarAsync(lead.Id, TipoAnexoConta.Pdf, "%PDF-teste"u8.ToArray(), CancellationToken.None);
        var anexo = AnexoConta.Criar(lead.Id, TipoAnexoConta.Pdf, 10, caminho, criadoEm, criadoEm.AddDays(90));
        contexto.AnexosConta.Add(anexo);
        await contexto.SaveChangesAsync();
        return lead;
    }

    [Fact]
    public async Task ExecutarAsync_LeadAntigoNaoConvertido_AnonimizaEApagaAnexo()
    {
        var (job, contexto, armazenamento, agora) = CriarCenario();
        var lead = await AdicionarLeadComAnexoAsync(contexto, armazenamento, agora.AddMonths(-25));
        var anexo = await contexto.AnexosConta.SingleAsync(a => a.LeadId == lead.Id);

        await job.ExecutarAsync(CancellationToken.None);

        var leadAtualizado = await contexto.Leads.SingleAsync(l => l.Id == lead.Id);
        Assert.Equal("[expurgado]", leadAtualizado.Nome);
        Assert.Equal("[expurgado]", leadAtualizado.Telefone);
        Assert.Equal("[expurgado]", leadAtualizado.Email);
        Assert.Equal(agora, leadAtualizado.ExpurgadoEm);
        var anexoAtualizado = await contexto.AnexosConta.SingleAsync(a => a.Id == anexo.Id);
        Assert.Equal(agora, anexoAtualizado.DescartadoEm);
        Assert.Null(await armazenamento.LerAsync(anexo.CaminhoArmazenamento, CancellationToken.None));
    }

    [Fact]
    public async Task ExecutarAsync_LeadDentroDoPrazo_NaoToca()
    {
        var (job, contexto, armazenamento, agora) = CriarCenario();
        var lead = await AdicionarLeadComAnexoAsync(contexto, armazenamento, agora.AddMonths(-1));

        await job.ExecutarAsync(CancellationToken.None);

        var leadAtualizado = await contexto.Leads.SingleAsync(l => l.Id == lead.Id);
        Assert.Equal("Maria", leadAtualizado.Nome);
        Assert.Null(leadAtualizado.ExpurgadoEm);
    }

    [Fact]
    public async Task ExecutarAsync_LeadConvertidoAntigo_NaoToca()
    {
        var (job, contexto, armazenamento, agora) = CriarCenario();
        var lead = await AdicionarLeadComAnexoAsync(contexto, armazenamento, agora.AddMonths(-25));
        typeof(LeadEntidade).GetProperty(nameof(LeadEntidade.Status))!.SetValue(lead, StatusLead.Convertido);
        await contexto.SaveChangesAsync();

        await job.ExecutarAsync(CancellationToken.None);

        var leadAtualizado = await contexto.Leads.SingleAsync(l => l.Id == lead.Id);
        Assert.Equal("Maria", leadAtualizado.Nome);
        Assert.Null(leadAtualizado.ExpurgadoEm);
    }
}
