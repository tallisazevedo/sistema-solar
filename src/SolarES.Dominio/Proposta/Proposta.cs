namespace SolarES.Dominio.Proposta;

public sealed class Proposta : EntidadeBase
{
    public Guid SimulacaoId { get; set; }
    public required string Numero { get; set; }
    public Guid ConfiguracaoVersaoId { get; set; }
    public DateTimeOffset ValidaAte { get; set; }
    public string? ArquivoPdfUrl { get; set; }
    public DateTimeOffset? EnviadaEm { get; set; }
    public CanalEnvio? Canal { get; set; }
    public StatusProposta Status { get; set; }
    public DateTimeOffset? AceitaEm { get; set; }
    public DateTimeOffset? PerdidaEm { get; set; }
    public string? MotivoPerda { get; set; }

    /// <summary>Aceite do cliente. Recusa fora de Emitida e depois de ValidaAte -- exatamente em ValidaAte e' permitido.</summary>
    public void Aceitar(DateTimeOffset agora)
    {
        if (Status is StatusProposta.Aceita or StatusProposta.Perdida)
            throw new TransicaoInvalidaException($"Proposta '{Numero}' ja esta em status terminal ({Status}) e nao pode ser aceita.");
        if (agora > ValidaAte)
            throw new TransicaoInvalidaException($"Proposta '{Numero}' venceu em {ValidaAte:dd/MM/yyyy}. Emita uma nova proposta.");
        Status = StatusProposta.Aceita;
        AceitaEm = agora;
        AtualizadoEm = agora;
    }

    /// <summary>Perda registrada pelo vendedor, com motivo opcional. Nao depende do relogio -- so' de status nao terminal.</summary>
    public void MarcarPerdida(DateTimeOffset agora, string? motivo)
    {
        if (Status is StatusProposta.Aceita or StatusProposta.Perdida)
            throw new TransicaoInvalidaException($"Proposta '{Numero}' ja esta em status terminal ({Status}) e nao pode ser marcada como perdida.");
        Status = StatusProposta.Perdida;
        PerdidaEm = agora;
        MotivoPerda = motivo;
        AtualizadoEm = agora;
    }
}
