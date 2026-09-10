using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SolarES.Aplicacao.Propostas;

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
        // PDF ainda gerando (T21, job assincrono) e "nao encontrado ainda", nao
        // "entrada invalida" -- 404, nao 400.
        if (exception is PropostaAindaNaoGeradaException aindaNaoGerada)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "PDF ainda nao disponivel.",
                Detail = aindaNaoGerada.Message,
            }, cancellationToken);

            return true;
        }

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
