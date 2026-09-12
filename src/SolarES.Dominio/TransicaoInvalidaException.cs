namespace SolarES.Dominio;

/// <summary>
/// O agregado existe e a entrada e valida, mas o estado atual nao permite a operacao
/// pedida (proposta vencida, status terminal, calibracao pendente) -- mapeia para 409,
/// nunca para 400 (que e' erro de entrada) ou 404 (que e' "nao existe").
/// </summary>
public sealed class TransicaoInvalidaException(string message) : Exception(message);
