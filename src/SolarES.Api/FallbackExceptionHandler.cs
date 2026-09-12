using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SolarES.Api;

/// <summary>
/// Ultima linha de defesa: qualquer excecao que nao seja um caso conhecido de
/// ExcecaoDeValidacaoDominioHandler cai aqui. Fora de Development, o corpo nunca leva
/// stack trace, nome de classe ou mensagem interna -- so um titulo generico e o traceId
/// para correlacionar com o log.
/// </summary>
public sealed partial class FallbackExceptionHandler(
    IHostEnvironment ambiente,
    IProblemDetailsService problemDetailsService,
    ILogger<FallbackExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogErroNaoTratado(logger, httpContext.Request.Method, httpContext.Request.Path, exception);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro interno.",
            Detail = ambiente.IsDevelopment()
                ? exception.ToString()
                : "Ocorreu um erro inesperado. Tente novamente em instantes.",
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });

        return true;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Erro nao tratado ao processar {Metodo} {Caminho}.")]
    private static partial void LogErroNaoTratado(ILogger logger, string metodo, string caminho, Exception exception);
}
