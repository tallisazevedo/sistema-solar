namespace SolarES.Aplicacao.Compartilhado;

public interface IExecutorTransacional
{
    Task<T> ExecutarAsync<T>(Func<CancellationToken, Task<T>> acao, CancellationToken ct);
}
