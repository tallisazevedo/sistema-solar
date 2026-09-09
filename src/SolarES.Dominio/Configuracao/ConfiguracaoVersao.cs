namespace SolarES.Dominio.Configuracao;

public sealed class ConfiguracaoVersao : EntidadeBase
{
    public int Numero { get; set; }
    public StatusConfiguracaoVersao Status { get; private set; }
    public ConfiguracaoCalculo Payload { get; private set; } = null!;
    public DateTimeOffset? PublicadaEm { get; private set; }
    public Guid? PublicadaPorUsuarioId { get; private set; }
    public string? Observacao { get; set; }

    private ConfiguracaoVersao()
    {
    }

    public static ConfiguracaoVersao CriarRascunho(int numero, ConfiguracaoCalculo payload, string? observacao = null) => new()
    {
        Id = Guid.NewGuid(),
        Numero = numero,
        Status = StatusConfiguracaoVersao.Rascunho,
        Payload = payload,
        Observacao = observacao,
    };

    /// <summary>Versao publicada e imutavel — falha se Status ja for Publicada.</summary>
    public void AtualizarPayload(ConfiguracaoCalculo novoPayload)
    {
        if (Status == StatusConfiguracaoVersao.Publicada)
        {
            throw new InvalidOperationException("Versao publicada e imutavel; nao e possivel alterar o payload.");
        }

        Payload = novoPayload;
    }

    public void Publicar(Guid usuarioId, DateTimeOffset momento)
    {
        if (Status != StatusConfiguracaoVersao.Rascunho)
        {
            throw new InvalidOperationException("Somente um rascunho pode ser publicado.");
        }

        Status = StatusConfiguracaoVersao.Publicada;
        PublicadaEm = momento;
        PublicadaPorUsuarioId = usuarioId;
    }

    public void Arquivar()
    {
        if (Status != StatusConfiguracaoVersao.Publicada)
        {
            throw new InvalidOperationException("Somente uma versao publicada pode ser arquivada.");
        }

        Status = StatusConfiguracaoVersao.Arquivada;
    }
}
