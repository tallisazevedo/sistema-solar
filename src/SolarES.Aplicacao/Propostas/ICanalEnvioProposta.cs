using SolarES.Dominio.Proposta;

namespace SolarES.Aplicacao.Propostas;

/// <summary>
/// Porta para o canal concreto de envio (SMTP, disco em dev, WhatsApp Cloud API). Devolve
/// o id da mensagem no provedor quando houver, ou lanca em caso de falha -- quem decide
/// retry e' o Hangfire (EnviarPropostaJob), nao o adaptador.
/// </summary>
public interface ICanalEnvioProposta
{
    CanalEnvio Canal { get; }
    Task<string?> EnviarAsync(string destino, string numeroProposta, byte[] pdf, CancellationToken ct);
}
