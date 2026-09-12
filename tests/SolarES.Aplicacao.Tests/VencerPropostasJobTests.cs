using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Identidade;
using SolarES.Dominio.Lead;
using SolarES.Dominio.Proposta;
using SolarES.Dominio.Simulacao;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;
using PropostaEntidade = SolarES.Dominio.Proposta.Proposta;
using SimulacaoEntidade = SolarES.Dominio.Simulacao.Simulacao;

namespace SolarES.Aplicacao.Tests;

public sealed class VencerPropostasJobTests
{
    private sealed record NotificacaoRecebida(IReadOnlyCollection<string> Destinatarios, string Assunto, string Corpo);

    private sealed class NotificadorInternoFalso : INotificadorInterno
    {
        public List<NotificacaoRecebida> Notificacoes { get; } = [];

        public Task EnviarEmailAsync(IReadOnlyCollection<string> destinatarios, string assunto, string corpo,
            CancellationToken ct)
        {
            Notificacoes.Add(new(destinatarios, assunto, corpo));
            return Task.CompletedTask;
        }
    }

    private static (VencerPropostasJob Job, SolarESDbContext Contexto, NotificadorInternoFalso Notificador,
        DateTimeOffset Agora) CriarCenario()
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contexto = new SolarESDbContext(options);
        var agora = new DateTimeOffset(2026, 9, 12, 12, 0, 0, TimeSpan.Zero);
        var notificador = new NotificadorInternoFalso();
        var job = new VencerPropostasJob(
            new EfPropostaRepository(contexto),
            new EfDadosNotificacaoVencimentoQuery(contexto),
            notificador,
            new FakeTimeProvider(agora));
        return (job, contexto, notificador, agora);
    }

    private static PropostaEntidade NovaProposta(DateTimeOffset validaAte, StatusProposta status,
        Guid simulacaoId, Guid? responsavelUsuarioId = null) => new()
        {
            Id = Guid.NewGuid(),
            SimulacaoId = simulacaoId,
            Numero = $"PROP-2026-{Random.Shared.Next(1, 9999):D4}",
            ConfiguracaoVersaoId = Guid.NewGuid(),
            ValidaAte = validaAte,
            Status = status,
            ResponsavelUsuarioId = responsavelUsuarioId,
            CriadoEm = validaAte.AddDays(-15),
            AtualizadoEm = validaAte.AddDays(-15),
        };

    private static SimulacaoEntidade NovaSimulacao(Guid? leadId = null) => new()
    {
        Id = Guid.NewGuid(),
        LeadId = leadId,
        ConfiguracaoVersaoId = Guid.NewGuid(),
        EntradasSnapshot = "{}",
        ResultadoSnapshot = "{}",
    };

    [Fact]
    public async Task ExecutarAsync_VenceEAvisaSomentePropostasEmitidasComValidadePassada()
    {
        var (job, contexto, notificador, agora) = CriarCenario();
        var vendedor = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Vendedor",
            Email = "vendedor@solares.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Vendedor,
            Ativo = true
        };
        var simulacaoVencida = NovaSimulacao();
        var simulacaoValida = NovaSimulacao();
        var simulacaoAceita = NovaSimulacao();
        var vencida = NovaProposta(agora.AddSeconds(-1), StatusProposta.Emitida, simulacaoVencida.Id, vendedor.Id);
        var valida = NovaProposta(agora, StatusProposta.Emitida, simulacaoValida.Id, vendedor.Id);
        var aceita = NovaProposta(agora.AddDays(-1), StatusProposta.Aceita, simulacaoAceita.Id, vendedor.Id);
        contexto.AddRange(vendedor, simulacaoVencida, simulacaoValida, simulacaoAceita, vencida, valida, aceita);
        await contexto.SaveChangesAsync();

        await job.ExecutarAsync(CancellationToken.None);

        Assert.Equal(StatusProposta.Vencida, vencida.Status);
        Assert.Equal(agora, vencida.VencidaEm);
        Assert.Equal(agora, vencida.VencimentoNotificadoEm);
        Assert.Equal(StatusProposta.Emitida, valida.Status);
        Assert.Equal(StatusProposta.Aceita, aceita.Status);
        Assert.Single(notificador.Notificacoes);
    }

    [Fact]
    public async Task ExecutarAsync_ExecutadoDuasVezes_NaoVenceNemAvisaNovamente()
    {
        var (job, contexto, notificador, agora) = CriarCenario();
        var vendedor = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Vendedor",
            Email = "vendedor@solares.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Vendedor,
            Ativo = true
        };
        var simulacao = NovaSimulacao();
        var proposta = NovaProposta(agora.AddDays(-1), StatusProposta.Emitida, simulacao.Id, vendedor.Id);
        contexto.AddRange(vendedor, simulacao, proposta);
        await contexto.SaveChangesAsync();

        await job.ExecutarAsync(CancellationToken.None);
        await job.ExecutarAsync(CancellationToken.None);

        Assert.Single(notificador.Notificacoes);
        Assert.Equal(agora, proposta.VencidaEm);
    }

    [Fact]
    public async Task ExecutarAsync_ComResponsavel_AvisaSomenteOResponsavelEIncluiCliente()
    {
        var (job, contexto, notificador, agora) = CriarCenario();
        var responsavel = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Ana",
            Email = "ana@solares.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Vendedor,
            Ativo = true
        };
        var outroVendedor = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Bia",
            Email = "bia@solares.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Vendedor,
            Ativo = true
        };
        var simulacaoId = Guid.NewGuid();
        var lead = Lead.Criar("Cliente Maria", "27999999999", "maria@exemplo.com", CanalPreferido.Email,
            simulacaoId, Guid.NewGuid(), [FinalidadeConsentimento.ContatoComercial], agora.AddDays(-20));
        var simulacao = NovaSimulacao(lead.Id);
        var proposta = NovaProposta(agora.AddDays(-1), StatusProposta.Emitida, simulacao.Id, responsavel.Id);
        contexto.AddRange(responsavel, outroVendedor, lead, simulacao, proposta);
        await contexto.SaveChangesAsync();

        await job.ExecutarAsync(CancellationToken.None);

        var notificacao = Assert.Single(notificador.Notificacoes);
        Assert.Equal([responsavel.Email], notificacao.Destinatarios);
        Assert.Contains(proposta.Numero, notificacao.Corpo);
        Assert.Contains(lead.Nome, notificacao.Corpo);
        Assert.Contains(proposta.ValidaAte.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), notificacao.Corpo);
    }

    [Fact]
    public async Task ExecutarAsync_SemResponsavel_AvisaVendedoresEDonosAtivos()
    {
        var (job, contexto, notificador, agora) = CriarCenario();
        var dono = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Dono",
            Email = "dono@solares.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Dono,
            Ativo = true
        };
        var vendedor = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Vendedor",
            Email = "vendedor@solares.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Vendedor,
            Ativo = true
        };
        var engenheiro = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Engenheiro",
            Email = "eng@solares.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Engenheiro,
            Ativo = true
        };
        var inativo = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Inativo",
            Email = "inativo@solares.com",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Vendedor,
            Ativo = false
        };
        var simulacao = NovaSimulacao();
        var proposta = NovaProposta(agora.AddDays(-1), StatusProposta.Emitida, simulacao.Id);
        contexto.AddRange(dono, vendedor, engenheiro, inativo, simulacao, proposta);
        await contexto.SaveChangesAsync();

        await job.ExecutarAsync(CancellationToken.None);

        var notificacao = Assert.Single(notificador.Notificacoes);
        Assert.Equal([dono.Email, vendedor.Email], notificacao.Destinatarios.Order());
        Assert.DoesNotContain("Cliente:", notificacao.Corpo);
    }
}
