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

    private static (EnviarPropostaJob Job, SolarESDbContext Contexto, PropostaEntidade Proposta, EnvioProposta Envio,
            CanalFalso Canal, DateTimeOffset Agora, FakeTimeProvider Relogio)
        CriarCenario(bool pdfGerado, bool falhaCanal = false, int limitePorDestino = 3, string destino = "cliente@exemplo.com")
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

        var envio = EnvioProposta.Criar(proposta.Id, CanalEnvio.Email, destino, agora);
        contexto.EnviosProposta.Add(envio);
        contexto.SaveChanges();

        var canal = new CanalFalso(falhaCanal);
        var relogio = new FakeTimeProvider(agora);
        var job = new EnviarPropostaJob(
            new EfEnvioPropostaRepository(contexto),
            new EfPropostaRepository(contexto),
            armazenamentoPdf,
            [canal],
            new ConfiguracaoLimiteEnvios(limitePorDestino, TimeSpan.FromHours(24)),
            relogio);

        return (job, contexto, proposta, envio, canal, agora, relogio);
    }

    [Fact]
    public async Task ExecutarAsync_CaminhoFeliz_MarcaEnviadoEPreencheEnviadaEmNaProposta()
    {
        var (job, contexto, proposta, envio, canal, _, _) = CriarCenario(pdfGerado: true);

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
        var (job, contexto, _, envio, _, _, _) = CriarCenario(pdfGerado: false);

        await Assert.ThrowsAsync<PropostaAindaNaoGeradaException>(
            () => job.ExecutarAsync(envio.Id, CancellationToken.None));

        var envioAtualizado = await contexto.EnviosProposta.SingleAsync(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvioProposta.Pendente, envioAtualizado.Status);
    }

    [Fact]
    public async Task ExecutarAsync_CanalFalha_MarcaFalhouEPropagaParaORetryDoHangfire()
    {
        var (job, contexto, _, envio, _, _, _) = CriarCenario(pdfGerado: true, falhaCanal: true);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => job.ExecutarAsync(envio.Id, CancellationToken.None));

        var envioAtualizado = await contexto.EnviosProposta.SingleAsync(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvioProposta.Falhou, envioAtualizado.Status);
        Assert.Equal("Falha simulada no canal.", envioAtualizado.UltimoErro);
        Assert.Equal(1, envioAtualizado.Tentativas);
    }

    [Fact]
    public async Task ExecutarAsync_DestinoJaNoLimiteNaJanela_NaoChamaOCanalEMarcaFalhouSemLancar()
    {
        const string destino = "repetido@exemplo.com";
        var (job, contexto, _, envio, canal, agora, _) = CriarCenario(pdfGerado: true, limitePorDestino: 2, destino: destino);

        // Dois envios ja confirmados pro mesmo destino dentro da janela -- o limite (2) ja foi atingido.
        contexto.EnviosProposta.Add(CriarEnvioEnviado(destino, agora.AddMinutes(-10)));
        contexto.EnviosProposta.Add(CriarEnvioEnviado(destino, agora.AddMinutes(-5)));
        contexto.SaveChanges();

        await job.ExecutarAsync(envio.Id, CancellationToken.None);

        Assert.Empty(canal.DestinosRecebidos);
        var envioAtualizado = await contexto.EnviosProposta.SingleAsync(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvioProposta.Falhou, envioAtualizado.Status);
        Assert.Contains("Limite", envioAtualizado.UltimoErro);
    }

    [Fact]
    public async Task ExecutarAsync_DestinoNoLimiteMasJanelaJaPassou_VoltaAChamarOCanal()
    {
        const string destino = "repetido@exemplo.com";
        var (job, contexto, _, envio, canal, agora, relogio) = CriarCenario(pdfGerado: true, limitePorDestino: 2, destino: destino);

        contexto.EnviosProposta.Add(CriarEnvioEnviado(destino, agora.AddHours(-30)));
        contexto.EnviosProposta.Add(CriarEnvioEnviado(destino, agora.AddHours(-25)));
        contexto.SaveChanges();
        relogio.AvancarPara(agora); // envio proprio (criado em "agora") esta fora da janela de 24h desses dois

        await job.ExecutarAsync(envio.Id, CancellationToken.None);

        Assert.Contains(destino, canal.DestinosRecebidos);
        var envioAtualizado = await contexto.EnviosProposta.SingleAsync(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvioProposta.Enviado, envioAtualizado.Status);
    }

    private static EnvioProposta CriarEnvioEnviado(string destino, DateTimeOffset enviadoEm)
    {
        var envio = EnvioProposta.Criar(Guid.NewGuid(), CanalEnvio.Email, destino, enviadoEm);
        envio.MarcarEnviado("msg-anterior", enviadoEm);
        return envio;
    }
}
