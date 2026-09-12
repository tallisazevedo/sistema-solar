using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Compartilhado;

namespace SolarES.Infraestrutura.Persistencia;

public sealed class EfExecutorTransacional(SolarESDbContext contexto) : IExecutorTransacional
{
    public async Task<T> ExecutarAsync<T>(Func<CancellationToken, Task<T>> acao, CancellationToken ct)
    {
        if (!contexto.Database.IsRelational()) return await acao(ct);
        await using var transacao = await contexto.Database.BeginTransactionAsync(ct);
        var resultado = await acao(ct);
        await transacao.CommitAsync(ct);
        return resultado;
    }
}
