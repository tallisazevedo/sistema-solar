using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Proposta;
using SolarES.Infraestrutura.Persistencia;
using SolarES.Infraestrutura.Persistencia.Repositorios;

namespace SolarES.Aplicacao.Tests;

public class WebhookWhatsAppAppServiceTests
{
    private static (WebhookWhatsAppAppService Servico, SolarESDbContext Contexto, EnvioProposta Envio, DateTimeOffset Agora)
        CriarCenario()
    {
        var options = new DbContextOptionsBuilder<SolarESDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contexto = new SolarESDbContext(options);

        var agora = new DateTimeOffset(2026, 9, 12, 10, 0, 0, TimeSpan.Zero);
        var envio = EnvioProposta.Criar(Guid.NewGuid(), CanalEnvio.Whatsapp, "27999999999", agora.AddMinutes(-5));
        envio.MarcarEnviado("wamid.abc123", agora.AddMinutes(-4));
        contexto.EnviosProposta.Add(envio);
        contexto.SaveChanges();

        var servico = new WebhookWhatsAppAppService(new EfEnvioPropostaRepository(contexto), new FakeTimeProvider(agora));
        return (servico, contexto, envio, agora);
    }

    private static string PayloadStatus(string idMensagem, string status, string? erroTitulo = null)
    {
        var erros = erroTitulo is null ? "" : $",\"errors\":[{{\"title\":\"{erroTitulo}\"}}]";
        return "{\"entry\":[{\"changes\":[{\"value\":{\"statuses\":[{\"id\":\""
            + idMensagem + "\",\"status\":\"" + status + "\"" + erros + "}]}}]}]}";
    }

    [Fact]
    public async Task ProcessarAsync_StatusDelivered_MarcaEntregueComEntregueEm()
    {
        var (servico, contexto, envio, agora) = CriarCenario();

        await servico.ProcessarAsync(PayloadStatus("wamid.abc123", "delivered"), CancellationToken.None);

        var atualizado = await contexto.EnviosProposta.SingleAsync(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvioProposta.Entregue, atualizado.Status);
        Assert.Equal(agora, atualizado.EntregueEm);
    }

    [Fact]
    public async Task ProcessarAsync_StatusFailed_MarcaFalhouComMensagemDoErro()
    {
        var (servico, contexto, envio, _) = CriarCenario();

        await servico.ProcessarAsync(PayloadStatus("wamid.abc123", "failed", "Numero invalido"), CancellationToken.None);

        var atualizado = await contexto.EnviosProposta.SingleAsync(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvioProposta.Falhou, atualizado.Status);
        Assert.Equal("Numero invalido", atualizado.UltimoErro);
    }

    [Fact]
    public async Task ProcessarAsync_IdMensagemDesconhecido_NaoAlteraNadaNemLanca()
    {
        var (servico, contexto, envio, _) = CriarCenario();

        await servico.ProcessarAsync(PayloadStatus("wamid.outro-id", "delivered"), CancellationToken.None);

        var atualizado = await contexto.EnviosProposta.SingleAsync(e => e.Id == envio.Id);
        Assert.Equal(StatusEnvioProposta.Enviado, atualizado.Status);
    }

    [Fact]
    public async Task ProcessarAsync_PayloadSemStatuses_NaoLanca()
    {
        var (servico, _, _, _) = CriarCenario();

        await servico.ProcessarAsync("""{"entry":[{"changes":[{"value":{}}]}]}""", CancellationToken.None);
    }
}
