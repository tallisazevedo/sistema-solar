using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SolarES.Api;

/// <summary>
/// Premissa&lt;T&gt;, ConfiguracaoVersao etc. validam a si mesmos no construtor/metodo
/// (ArgumentException, InvalidOperationException) -- sem isso, viraria 500 em vez de
/// 400. Nao duplica a regra do Dominio, so traduz a excecao ja lancada.
/// </summary>
public sealed class ExcecaoDeValidacaoDominioHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not (ArgumentException or InvalidOperationException))
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Entrada invalida.",
            Detail = exception.Message,
        }, cancellationToken);

        return true;
    }
}
