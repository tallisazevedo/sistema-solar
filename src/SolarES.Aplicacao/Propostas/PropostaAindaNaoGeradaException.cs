namespace SolarES.Aplicacao.Propostas;

/// <summary>
/// O registro da Proposta existe mas o job de geracao do PDF (T21, Hangfire) ainda
/// nao terminou (ou ainda nao rodou). Distinto de "nao encontrada" -- o controller
/// mapeia os dois pra 404, mas a mensagem ajuda quem esta depurando.
/// </summary>
public sealed class PropostaAindaNaoGeradaException(string numero)
    : Exception($"O PDF da proposta '{numero}' ainda esta sendo gerado. Tente novamente em instantes.")
{
    public string Numero { get; } = numero;
}
