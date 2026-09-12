using Microsoft.AspNetCore.Mvc;

namespace SolarES.Api;

/// <summary>
/// Limite de tamanho de corpo por requisicao: um teto global e apertado para JSON, e um
/// teto proprio, maior, so' para o multipart de captura de lead (que carrega o anexo da
/// conta). A checagem usa Content-Length e roda antes de qualquer model binding -- uma
/// requisicao grande demais nunca chega no controller, no motor ou no armazenamento.
/// </summary>
public static class LimiteTamanhoRequisicaoExtensions
{
    public static IApplicationBuilder UseLimiteTamanhoRequisicaoPublica(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var caminho = context.Request.Path.Value ?? string.Empty;
            var ehSuperficiePublica = caminho.Contains("/api/publico/", StringComparison.OrdinalIgnoreCase)
                || caminho.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase);
            if (!ehSuperficiePublica)
            {
                await next();
                return;
            }

            var configuracao = context.RequestServices.GetRequiredService<IConfiguration>();
            var ehCapturaLead = caminho.EndsWith("/lead", StringComparison.OrdinalIgnoreCase);

            var limiteBytes = ehCapturaLead
                ? configuracao.GetValue<long?>("Requisicoes:LimiteMultipartLeadBytes") ?? 10_500_000
                : configuracao.GetValue<long?>("Requisicoes:LimiteCorpoJsonBytes") ?? 64_000;

            if (context.Request.ContentLength is { } tamanho && tamanho > limiteBytes)
            {
                await EscreverRespostaAsync(context, limiteBytes, ehCapturaLead);
                return;
            }

            await next();
        });
    }

    private static async Task EscreverRespostaAsync(HttpContext context, long limiteBytes, bool ehCapturaLead)
    {
        context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;

        var limiteMb = Math.Round(limiteBytes / 1_000_000.0, 1);
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status413PayloadTooLarge,
            Title = "Requisicao grande demais.",
            Detail = ehCapturaLead
                ? $"O arquivo e os dados enviados ultrapassam o limite de {limiteMb} MB. Formatos aceitos: PDF, JPG e PNG."
                : $"O corpo da requisicao ultrapassa o limite de {limiteMb} MB.",
        };
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails,
        });
    }
}
