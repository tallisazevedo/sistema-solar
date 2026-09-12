using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio;

namespace SolarES.Api;

/// <summary>
/// Premissa&lt;T&gt;, ConfiguracaoVersao etc. validam a si mesmos no construtor/metodo
/// (ArgumentException, InvalidOperationException) -- sem isso, viraria 500 em vez de
/// 400. Nao duplica a regra do Dominio, so traduz a excecao ja lancada.
/// </summary>
public sealed class ExcecaoDeValidacaoDominioHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // PDF ainda gerando (T21, job assincrono) e "nao encontrado ainda", nao
        // "entrada invalida" -- 404, nao 400.
        if (exception is PropostaAindaNaoGeradaException aindaNaoGerada)
        {
            return await EscreverAsync(httpContext, exception, StatusCodes.Status404NotFound, "PDF ainda nao disponivel.", aindaNaoGerada.Message, cancellationToken);
        }

        // Entrada valida, mas o estado atual do agregado recusa a operacao (proposta
        // vencida, status terminal, calibracao pendente) -- 409, nao 400.
        if (exception is TransicaoInvalidaException conflito)
        {
            return await EscreverAsync(httpContext, exception, StatusCodes.Status409Conflict, "Conflito de estado.", conflito.Message, cancellationToken);
        }

        if (exception is not (ArgumentException or InvalidOperationException))
        {
            return false;
        }

        return await EscreverAsync(httpContext, exception, StatusCodes.Status400BadRequest, "Entrada invalida.", exception.Message, cancellationToken);
    }

    private async ValueTask<bool> EscreverAsync(HttpContext httpContext, Exception exception, int status, string titulo, string detalhe, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = status;

        var problemDetails = new ProblemDetails { Status = status, Title = titulo, Detail = detalhe };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });

        return true;
    }
}
