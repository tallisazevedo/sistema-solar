using Hangfire;
using Hangfire.InMemory;
using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Premissas;
using SolarES.Dominio.Proposta;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;

namespace SolarES.Aplicacao.Tests;

public class EnvioPropostaAppServiceTests
{
    private sealed class CanalFalso : ICanalEnvioProposta
    {
        public CanalEnvio Canal => CanalEnvio.Email;
        public int ChamadasRecebidas { get; private set; }
        public Task<string?> EnviarAsync(string destino, string numeroProposta, byte[] pdf, CancellationToken ct)
        {
            ChamadasRecebidas++;
            return Task.FromResult<string?>("msg-falsa");
        }
    }

    private static (EnvioPropostaAppService Servico, SolarESDbContext Contexto, PropostaEntidade Proposta, CanalFalso Canal)
        CriarCenario(bool comPremissaProvisoria)
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contexto = new SolarESDbContext(options);

        var agora = DateTimeOffset.UtcNow;
        var payload = comPremissaProvisoria
            ? ConfiguracaoCalculoBaseline.Criar()
            : ConfiguracaoConfirmada();
        var configuracaoVersao = ConfiguracaoVersao.CriarRascunho(1, payload);
        configuracaoVersao.Publicar(Guid.NewGuid(), agora);
        contexto.ConfiguracoesVersao.Add(configuracaoVersao);

        var proposta = new PropostaEntidade
        {
            Id = Guid.NewGuid(),
            SimulacaoId = Guid.NewGuid(),
            Numero = "PROP-2026-0001",
            ConfiguracaoVersaoId = configuracaoVersao.Id,
            ValidaAte = agora.AddDays(15),
            Status = StatusProposta.Emitida,
            CriadoEm = agora,
            AtualizadoEm = agora,
        };
        contexto.Propostas.Add(proposta);
        contexto.SaveChanges();

        var jobs = new BackgroundJobClient(new InMemoryStorage());
        var canal = new CanalFalso();
        var servico = new EnvioPropostaAppService(
            new EfPropostaRepository(contexto),
            new ConfiguracaoVersaoRepository(contexto),
            new EfEnvioPropostaRepository(contexto),
            jobs,
            TimeProvider.System);

        return (servico, contexto, proposta, canal);
    }

    private static ConfiguracaoCalculo ConfiguracaoConfirmada()
    {
        var b = ConfiguracaoCalculoBaseline.Criar();
        return new(
            new(b.PerformanceRatio.Valor, OrigemPremissa.FontePublica),
            new(b.DegradacaoAnual.Valor, OrigemPremissa.FontePublica),
            new(b.InflacaoTarifaria.Valor, OrigemPremissa.FontePublica),
            new(b.TaxaDesconto.Valor, OrigemPremissa.FontePublica),
            new(b.HorizonteAnos.Valor, OrigemPremissa.FontePublica),
            new(b.OversizingMaximo.Valor, OrigemPremissa.FontePublica),
            new(b.FatorOrientacaoPadrao.Valor, OrigemPremissa.FontePublica),
            b.CronogramaFioB,
            new(b.EstrategiaFioBForaCronograma.Valor, OrigemPremissa.FontePublica),
            new(b.LimiteKwpRoteamentoHumano.Valor, OrigemPremissa.FontePublica),
            new(b.KitLitoral.Valor, OrigemPremissa.FontePublica),
            b.CustoDisponibilidadePorLigacao,
            new(b.TextosProposta.Valor, OrigemPremissa.FontePublica));
    }

    [Fact]
    public async Task Dada_PropostaSobPremissaProvisoria_Quando_SolicitaEnvio_Entao_RecusaSemPersistirNemChamarCanal()
    {
        var (servico, contexto, proposta, canal) = CriarCenario(comPremissaProvisoria: true);

        await Assert.ThrowsAsync<TransicaoInvalidaException>(
            () => servico.SolicitarEnvioAsync(proposta.Id, CanalEnvio.Email, "cliente@exemplo.com", CancellationToken.None));

        Assert.Empty(await contexto.EnviosProposta.ToListAsync());
        Assert.Equal(0, canal.ChamadasRecebidas);
    }

    [Fact]
    public async Task Dada_PropostaSobPremissaProvisoria_Quando_SolicitaEnvioPorWhatsApp_Entao_RecusaSemPersistirNemChamarCanal()
    {
        var (servico, contexto, proposta, canal) = CriarCenario(comPremissaProvisoria: true);

        await Assert.ThrowsAsync<TransicaoInvalidaException>(
            () => servico.SolicitarEnvioAsync(proposta.Id, CanalEnvio.Whatsapp, "27999999999", CancellationToken.None));

        Assert.Empty(await contexto.EnviosProposta.ToListAsync());
        Assert.Equal(0, canal.ChamadasRecebidas);
    }

    [Fact]
    public async Task Dada_PropostaSemPremissaProvisoria_Quando_SolicitaEnvio_Entao_CriaEnvioPendente()
    {
        var (servico, contexto, proposta, _) = CriarCenario(comPremissaProvisoria: false);

        var envio = await servico.SolicitarEnvioAsync(proposta.Id, CanalEnvio.Email, "cliente@exemplo.com", CancellationToken.None);

        Assert.Equal(StatusEnvioProposta.Pendente, envio.Status);
        Assert.Single(await contexto.EnviosProposta.ToListAsync());
    }

    [Fact]
    public async Task Dada_PropostaSobPremissaProvisoria_Quando_Reenvia_Entao_Recusa()
    {
        var (servico, contexto, proposta, _) = CriarCenario(comPremissaProvisoria: false);
        var envio = await servico.SolicitarEnvioAsync(proposta.Id, CanalEnvio.Email, "cliente@exemplo.com", CancellationToken.None);

        var versao = await contexto.ConfiguracoesVersao.SingleAsync();
        versao.Arquivar();
        var novaVersao = ConfiguracaoVersao.CriarRascunho(2, ConfiguracaoCalculoBaseline.Criar());
        novaVersao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        contexto.ConfiguracoesVersao.Add(novaVersao);
        proposta.ConfiguracaoVersaoId = novaVersao.Id;
        await contexto.SaveChangesAsync();

        await Assert.ThrowsAsync<TransicaoInvalidaException>(
            () => servico.ReenviarAsync(envio.Id, CancellationToken.None));
    }
}
