using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace SolarES.Api;

/// <summary>
/// Politicas de rate limit da superficie publica e do login, particionadas por IP. Um
/// endpoint autenticado do admin nunca entra aqui -- so os endpoints marcados com
/// [EnableRateLimiting("&lt;nome&gt;")] sao afetados.
/// </summary>
public static partial class RateLimitingExtensions
{
    public const string PublicoSimulacao = "publico-simulacao";
    public const string PublicoLead = "publico-lead";
    public const string PublicoEventos = "publico-eventos";
    public const string PublicoLeitura = "publico-leitura";
    public const string Login = "login";

    public static IServiceCollection AddRateLimitingPublico(this IServiceCollection services, IConfiguration configuracao)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            AdicionarPoliticaJanelaFixa(options, PublicoSimulacao, configuracao, "RateLimit:PublicoSimulacao", limitePadrao: 20, janelaMinutosPadrao: 10);
            AdicionarPoliticaJanelaFixa(options, PublicoLead, configuracao, "RateLimit:PublicoLead", limitePadrao: 5, janelaMinutosPadrao: 60);
            AdicionarPoliticaJanelaFixa(options, PublicoEventos, configuracao, "RateLimit:PublicoEventos", limitePadrao: 60, janelaMinutosPadrao: 10);
            AdicionarPoliticaJanelaFixa(options, PublicoLeitura, configuracao, "RateLimit:PublicoLeitura", limitePadrao: 120, janelaMinutosPadrao: 1);
            AdicionarPoliticaJanelaFixa(options, Login, configuracao, "RateLimit:Login", limitePadrao: 10, janelaMinutosPadrao: 15);

            options.OnRejected = async (context, cancellationToken) =>
            {
                var segundosParaEsperar = 60;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    segundosParaEsperar = (int)Math.Ceiling(retryAfter.TotalSeconds);
                }
                context.HttpContext.Response.Headers.RetryAfter =
                    segundosParaEsperar.ToString(System.Globalization.CultureInfo.InvariantCulture);

                var politica = context.HttpContext.GetEndpoint()?
                    .Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName ?? "desconhecida";
                var ipTruncado = TruncarIp(context.HttpContext.Connection.RemoteIpAddress);
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("SolarES.Api.RateLimiting");
                LogRateLimitExcedido(logger, politica, ipTruncado);

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Muitas requisicoes.",
                    Detail = $"Tente novamente em {segundosParaEsperar} segundos.",
                };
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                problemDetails.Extensions["retryAfterSegundos"] = segundosParaEsperar;

                var problemDetailsService = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = problemDetails,
                });
            };
        });

        return services;
    }

    private static void AdicionarPoliticaJanelaFixa(
        RateLimiterOptions options, string nomePolitica, IConfiguration configuracao, string chaveConfiguracao,
        int limitePadrao, int janelaMinutosPadrao)
    {
        var limite = configuracao.GetValue<int?>($"{chaveConfiguracao}:Limite") ?? limitePadrao;
        var janela = TimeSpan.FromMinutes(configuracao.GetValue<int?>($"{chaveConfiguracao}:JanelaMinutos") ?? janelaMinutosPadrao);

        options.AddPolicy(nomePolitica, httpContext => RateLimitPartition.GetFixedWindowLimiter(
            ChaveDoIp(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limite,
                Window = janela,
                QueueLimit = 0,
            }));
    }

    private static string ChaveDoIp(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

    /// <summary>Mascara o ultimo octeto/64 bits do IP antes de logar -- nunca o endereco completo.</summary>
    private static string TruncarIp(IPAddress? ip)
    {
        if (ip is null)
        {
            return "desconhecido";
        }

        var bytes = ip.GetAddressBytes();
        if (bytes.Length == 4)
        {
            bytes[3] = 0;
            return $"{new IPAddress(bytes)}/24";
        }

        if (bytes.Length == 16)
        {
            for (var indice = 8; indice < 16; indice++)
            {
                bytes[indice] = 0;
            }

            return $"{new IPAddress(bytes)}/64";
        }

        return "desconhecido";
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Rate limit excedido: politica={Politica} ip={IpTruncado}")]
    private static partial void LogRateLimitExcedido(ILogger logger, string politica, string ipTruncado);
}
