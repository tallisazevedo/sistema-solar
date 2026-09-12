namespace SolarES.Dominio.Proposta;

public sealed class EnvioProposta : EntidadeBase
{
    private EnvioProposta() { }

    public Guid PropostaId { get; private set; }
    public CanalEnvio Canal { get; private set; }
    public string Destino { get; private set; } = null!;
    public StatusEnvioProposta Status { get; private set; }
    public string? IdMensagemProvedor { get; private set; }
    public int Tentativas { get; private set; }
    public string? UltimoErro { get; private set; }
    public DateTimeOffset? EnviadoEm { get; private set; }
    public DateTimeOffset? EntregueEm { get; private set; }

    public static EnvioProposta Criar(Guid propostaId, CanalEnvio canal, string destino, DateTimeOffset momento)
    {
        if (propostaId == Guid.Empty) throw new ArgumentException("Proposta do envio é obrigatória.");
        if (string.IsNullOrWhiteSpace(destino)) throw new ArgumentException("Destino do envio é obrigatório.");
        return new()
        {
            Id = Guid.NewGuid(), PropostaId = propostaId, Canal = canal, Destino = destino.Trim(),
            Status = StatusEnvioProposta.Pendente, Tentativas = 0,
            CriadoEm = momento, AtualizadoEm = momento,
        };
    }

    public void MarcarEnviado(string? idMensagemProvedor, DateTimeOffset momento)
    {
        Status = StatusEnvioProposta.Enviado;
        IdMensagemProvedor = idMensagemProvedor;
        EnviadoEm = momento;
        Tentativas++;
        AtualizadoEm = momento;
    }

    public void MarcarFalhou(string erro, DateTimeOffset momento)
    {
        Status = StatusEnvioProposta.Falhou;
        UltimoErro = erro;
        Tentativas++;
        AtualizadoEm = momento;
    }

    /// <summary>Confirmacao de entrega via webhook do provedor (T25.2 -- WhatsApp).</summary>
    public void MarcarEntregue(DateTimeOffset momento)
    {
        Status = StatusEnvioProposta.Entregue;
        EntregueEm = momento;
        AtualizadoEm = momento;
    }
}
