namespace SolarES.Aplicacao.Propostas;

public interface INotificadorInterno
{
    Task EnviarEmailAsync(IReadOnlyCollection<string> destinatarios, string assunto, string corpo,
        CancellationToken ct);
}
