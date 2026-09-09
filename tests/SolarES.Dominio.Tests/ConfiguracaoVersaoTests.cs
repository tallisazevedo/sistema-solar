using SolarES.Dominio.Configuracao;

namespace SolarES.Dominio.Tests;

public class ConfiguracaoVersaoTests
{
    private static ConfiguracaoVersao CriarRascunho() =>
        ConfiguracaoVersao.CriarRascunho(1, ConfiguracaoCalculoBaseline.Criar());

    [Fact]
    public void CriarRascunho_ComecaComoRascunho()
    {
        var versao = CriarRascunho();

        Assert.Equal(StatusConfiguracaoVersao.Rascunho, versao.Status);
        Assert.Null(versao.PublicadaEm);
    }

    [Fact]
    public void Publicar_NumRascunho_MudaStatusEGravaMetadados()
    {
        var versao = CriarRascunho();
        var usuarioId = Guid.NewGuid();
        var momento = DateTimeOffset.UtcNow;

        versao.Publicar(usuarioId, momento);

        Assert.Equal(StatusConfiguracaoVersao.Publicada, versao.Status);
        Assert.Equal(usuarioId, versao.PublicadaPorUsuarioId);
        Assert.Equal(momento, versao.PublicadaEm);
    }

    [Fact]
    public void Publicar_NumaJaPublicada_Lanca()
    {
        var versao = CriarRascunho();
        versao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => versao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Publicar_NumaArquivada_Lanca()
    {
        var versao = CriarRascunho();
        versao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        versao.Arquivar();

        Assert.Throws<InvalidOperationException>(() => versao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void AtualizarPayload_NumRascunho_Funciona()
    {
        var versao = CriarRascunho();
        var novoPayload = ConfiguracaoCalculoBaseline.Criar();

        versao.AtualizarPayload(novoPayload);

        Assert.Same(novoPayload, versao.Payload);
    }

    [Fact]
    public void AtualizarPayload_NumaVersaoPublicada_Lanca()
    {
        var versao = CriarRascunho();
        versao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() => versao.AtualizarPayload(ConfiguracaoCalculoBaseline.Criar()));
    }

    [Fact]
    public void Arquivar_NumaPublicada_Funciona()
    {
        var versao = CriarRascunho();
        versao.Publicar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        versao.Arquivar();

        Assert.Equal(StatusConfiguracaoVersao.Arquivada, versao.Status);
    }

    [Fact]
    public void Arquivar_NumRascunho_Lanca()
    {
        var versao = CriarRascunho();

        Assert.Throws<InvalidOperationException>(versao.Arquivar);
    }
}
