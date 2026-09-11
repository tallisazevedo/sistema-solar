using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Proposta;
using SolarES.Dominio.Simulacao;
using SolarES.Infraestrutura.Pdf;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;
using SimulacaoEntidade = SolarES.Dominio.Simulacao.Simulacao;

namespace SolarES.Aplicacao.Tests;

public class GerarPdfPropostaJobTests
{
    static GerarPdfPropostaJobTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
    }

    private static readonly ResultadoSimulacao Resultado = new(
        PotenciaInstaladaKwp: 4.4m,
        QuantidadeModulos: 8,
        AreaNecessariaM2: 20m,
        CoberturaPercentual: 100m,
        Capex: 18480m,
        EconomiaMensalAno1: 325.16m,
        PaybackMesesSimples: 53,
        PaybackMesesDescontado: 76,
        Tir: 0.27m,
        Vpl: 34004.94m,
        RoteadaParaHumano: false,
        MotivoRoteamento: null,
        KitLitoral: false,
        InstalacaoRecomendada: true,
        Projecao: Enumerable.Range(1, 25)
            .Select(ano => new AnoProjecao(ano, 2025 + ano, 3000m - ano * 10m, 1200m - ano * 15m, 1200m - ano * 15m))
            .ToList());

    private sealed class AmbienteFalso(string diretorioRaiz) : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "SolarES.Aplicacao.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = diretorioRaiz;
        public string EnvironmentName { get; set; } = "Testing";
    }

    private static (GerarPdfPropostaJob Job, SolarESDbContext Contexto, PropostaEntidade Proposta, string DiretorioPdf) CriarCenario()
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contexto = new SolarESDbContext(options);

        var agora = DateTimeOffset.UtcNow;
        var configuracaoVersao = ConfiguracaoVersao.CriarRascunho(1, ConfiguracaoCalculoBaseline.Criar());
        configuracaoVersao.Publicar(Guid.NewGuid(), agora);
        contexto.ConfiguracoesVersao.Add(configuracaoVersao);

        var simulacao = new SimulacaoEntidade
        {
            Id = Guid.NewGuid(),
            ConfiguracaoVersaoId = configuracaoVersao.Id,
            EntradasSnapshot = "{}",
            ResultadoSnapshot = JsonSerializer.Serialize(Resultado),
            PotenciaKwp = Resultado.PotenciaInstaladaKwp,
            QuantidadeModulos = Resultado.QuantidadeModulos,
            Capex = Resultado.Capex,
            EconomiaMensalAno1 = Resultado.EconomiaMensalAno1,
            PaybackMeses = Resultado.PaybackMesesSimples,
            Tir = Resultado.Tir,
            Vpl = Resultado.Vpl,
            CoberturaPercentual = Resultado.CoberturaPercentual,
            RoteadaParaHumano = Resultado.RoteadaParaHumano,
            CriadoEm = agora,
            AtualizadoEm = agora,
        };
        contexto.Simulacoes.Add(simulacao);

        var proposta = new PropostaEntidade
        {
            Id = Guid.NewGuid(),
            SimulacaoId = simulacao.Id,
            Numero = "PROP-2026-0001",
            ConfiguracaoVersaoId = configuracaoVersao.Id,
            ValidaAte = agora.AddDays(15),
            Status = StatusProposta.Emitida,
            CriadoEm = agora,
            AtualizadoEm = agora,
        };
        contexto.Propostas.Add(proposta);
        contexto.SaveChanges();

        var diretorioRaiz = Directory.CreateTempSubdirectory("solares-pdf-teste-").FullName;
        var diretorioPdf = Path.Combine(diretorioRaiz, "App_Data", "propostas");
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection([new("Pdfs:DiretorioArmazenamento", "App_Data/propostas")])
            .Build();

        var job = new GerarPdfPropostaJob(
            new EfPropostaRepository(contexto),
            new EfSimulacaoRepository(contexto),
            new ConfiguracaoVersaoRepository(contexto),
            new GeradorPdfProposta(),
            new ArmazenamentoPdfEmDisco(configuracao, new AmbienteFalso(diretorioRaiz)));

        return (job, contexto, proposta, diretorioPdf);
    }

    [Fact]
    public async Task ExecutarAsync_CaminhoFeliz_GravaArquivoEAtualizaArquivoPdfUrl()
    {
        var (job, contexto, proposta, diretorioPdf) = CriarCenario();

        await job.ExecutarAsync(proposta.Id, CancellationToken.None);

        var propostaAtualizada = await contexto.Propostas.SingleAsync(p => p.Id == proposta.Id);
        Assert.NotNull(propostaAtualizada.ArquivoPdfUrl);
        Assert.Equal(Path.Combine(diretorioPdf, $"{proposta.Numero}.pdf"), propostaAtualizada.ArquivoPdfUrl);

        var bytes = await File.ReadAllBytesAsync(propostaAtualizada.ArquivoPdfUrl!);
        Assert.True(bytes.Length > 0);
        Assert.Equal("%PDF"u8.ToArray(), bytes.Take(4).ToArray());
    }

    [Fact]
    public async Task ExecutarAsync_PropostaInexistente_Lanca()
    {
        var (job, _, _, _) = CriarCenario();

        // Prova que o Hangfire teria como reprocessar: uma falha na execucao propaga a
        // excecao em vez de ser engolida.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => job.ExecutarAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
