using SolarES.Dominio.Proposta;

namespace SolarES.Dominio.Tests;

public sealed class EnvioPropostaTests
{
    [Fact]
    public void Dado_DestinoEPropostaValidos_Quando_Cria_Entao_IniciaComoPendente()
    {
        var momento = DateTimeOffset.UtcNow;
        var envio = EnvioProposta.Criar(Guid.NewGuid(), CanalEnvio.Email, "cliente@exemplo.com", momento);

        Assert.Equal(StatusEnvioProposta.Pendente, envio.Status);
        Assert.Equal(0, envio.Tentativas);
        Assert.Equal("cliente@exemplo.com", envio.Destino);
    }

    [Fact]
    public void Dado_DestinoVazio_Quando_Cria_Entao_Recusa()
    {
        Assert.Throws<ArgumentException>(() => EnvioProposta.Criar(Guid.NewGuid(), CanalEnvio.Email, " ", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Dado_EnvioPendente_Quando_MarcaEnviado_Entao_PreencheEnviadoEmEIncrementaTentativas()
    {
        var momento = DateTimeOffset.UtcNow;
        var envio = EnvioProposta.Criar(Guid.NewGuid(), CanalEnvio.Email, "cliente@exemplo.com", momento);

        var enviadoEm = momento.AddSeconds(5);
        envio.MarcarEnviado("msg-123", enviadoEm);

        Assert.Equal(StatusEnvioProposta.Enviado, envio.Status);
        Assert.Equal("msg-123", envio.IdMensagemProvedor);
        Assert.Equal(enviadoEm, envio.EnviadoEm);
        Assert.Equal(1, envio.Tentativas);
    }

    [Fact]
    public void Dado_EnvioPendente_Quando_MarcaFalhou_Entao_PreencheUltimoErroEIncrementaTentativas()
    {
        var momento = DateTimeOffset.UtcNow;
        var envio = EnvioProposta.Criar(Guid.NewGuid(), CanalEnvio.Email, "cliente@exemplo.com", momento);

        envio.MarcarFalhou("Conexao SMTP recusada.", momento.AddSeconds(2));

        Assert.Equal(StatusEnvioProposta.Falhou, envio.Status);
        Assert.Equal("Conexao SMTP recusada.", envio.UltimoErro);
        Assert.Equal(1, envio.Tentativas);
    }

    [Fact]
    public void Dado_EnvioEnviado_Quando_MarcaEntregue_Entao_PreencheEntregueEm()
    {
        var momento = DateTimeOffset.UtcNow;
        var envio = EnvioProposta.Criar(Guid.NewGuid(), CanalEnvio.Whatsapp, "27999999999", momento);
        envio.MarcarEnviado("wamid.123", momento.AddSeconds(1));

        var entregueEm = momento.AddSeconds(10);
        envio.MarcarEntregue(entregueEm);

        Assert.Equal(StatusEnvioProposta.Entregue, envio.Status);
        Assert.Equal(entregueEm, envio.EntregueEm);
    }
}
