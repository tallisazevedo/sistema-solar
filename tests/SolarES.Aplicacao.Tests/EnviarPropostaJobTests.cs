using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Proposta;
using SolarES.Infraestrutura.Pdf;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Aplicacao.Tests;

public class EnviarPropostaJobTests
{
    private sealed class AmbienteFalso(string diretorioRaiz) : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "SolarES.Aplicacao.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = diretorioRaiz;
        public string EnvironmentName { get; set; } = "Testing";
    }

    private sealed class CanalFalso(bool falha) : ICanalEnvioProposta
    {
        public CanalEnvio Canal => CanalEnvio.Email;
        public List<string> DestinosRecebidos { get; } = [];
        public Task<string?> EnviarAsync(string destino, string numeroProposta, byte[] pdf, CancellationToken ct)
        {
            if (falha) throw new InvalidOperationException("Falha simulada no canal.");
            DestinosRecebidos.Add(destino);
            return Task.FromResult<string?>("msg-123");
        }
    }

    private static (EnviarPropostaJob Job, SolarESDbContext Contexto, PropostaEntidade Proposta, EnvioProposta Envio, CanalFalso Canal)
        CriarCenario(bool pdfGerado, bool falhaCanal = false)
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contexto = new SolarESDbContext(options);

        var agora = DateTimeOffset.UtcNow;
        var proposta = new PropostaEntidade
        {
            Id = Guid.NewGuid(),
            SimulacaoId = Guid.NewGuid(),
            Numero = "PROP-2026-0001",
            ConfiguracaoVersaoId = Guid.NewGuid(),
            ValidaAte = agora.AddDays(15),
            Status = StatusProposta.Emitida,
            ArquivoPdfUrl = null,
            CriadoEm = agora,
            AtualizadoEm = agora,
        };
        contexto.Propostas.Add(proposta);

        var diretorioRaiz = Directory.CreateTempSubdirectory("solares-envio-teste-").FullName;
        var diretorioPdf = Path.Combine(diretorioRaiz, "App_Data", "propostas");
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection([new("Pdfs:DiretorioArmazenamento", "App_Data/propostas")])
            .Build();
        var armazenamentoPdf = new ArmazenamentoPdfEmDisco(configuracao, new AmbienteFalso(diretorioRaiz));

        if (pdfGerado)
        {
            Directory.CreateDirectory(diretorioPdf);
            var caminhoPdf = Path.Combine(diretorioPdf, $"{proposta.Numero}.pdf");
            File.WriteAllBytes(caminhoPdf, "%PDF-1.4 conteudo"u8.ToArray());
            proposta.ArquivoPdfUrl = caminhoPdf;
        }

        var envio = EnvioProposta.Criar(proposta.Id, CanalEnvio.Email, "cliente@exemplo.com", agora);
        contexto.EnviosProposta.Add(envio);
        contexto.SaveChanges();

        var canal = new CanalFalso(falhaCanal);
        var job = new EnviarPropostaJob(
            new EfEnvioPropostaRepository(contexto),
            new EfPropostaRepository(contexto),
            armazenamentoPdf,
            [canal],
            new FakeTimeProvider(agora));

        return (job, contexto, proposta, envio, canal);
    }

    [Fact]
    public async Task ExecutarAsync_CaminhoFeliz_MarcaEnviadoEPreencheEnviadaEmNaProposta()
    {
        var (job, contexto, proposta, envio, canal) = CriarCenario(pdfGerado: true);

        await job.ExecutarAsync(envio.Id, CancellationToken.None);

        var envioAtualizado = await contexto.EnviosProposta.SingleAsync(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvioProposta.Enviado, envioAtualizado.Status);
        Assert.Equal("msg-123", envioAtualizado.IdMensagemProvedor);
        var propostaAtualizada = await contexto.Propostas.SingleAsync(p => p.Id == proposta.Id);
        Assert.NotNull(propostaAtualizada.EnviadaEm);
        Assert.Equal(CanalEnvio.Email, propostaAtualizada.Canal);
        Assert.Contains("cliente@exemplo.com", canal.DestinosRecebidos);
    }

    [Fact]
    public async Task ExecutarAsync_PdfAindaNaoGerado_Lanca()
    {
        var (job, contexto, _, envio, _) = CriarCenario(pdfGerado: false);

        await Assert.ThrowsAsync<PropostaAindaNaoGeradaException>(
            () => job.ExecutarAsync(envio.Id, CancellationToken.None));

        var envioAtualizado = await contexto.EnviosProposta.SingleAsync(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvioProposta.Pendente, envioAtualizado.Status);
    }

    [Fact]
    public async Task ExecutarAsync_CanalFalha_MarcaFalhouEPropagaParaORetryDoHangfire()
    {
        var (job, contexto, _, envio, _) = CriarCenario(pdfGerado: true, falhaCanal: true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => job.ExecutarAsync(envio.Id, CancellationToken.None));

        var envioAtualizado = await contexto.EnviosProposta.SingleAsync(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvioProposta.Falhou, envioAtualizado.Status);
        Assert.Equal("Falha simulada no canal.", envioAtualizado.UltimoErro);
        Assert.Equal(1, envioAtualizado.Tentativas);
    }
}
