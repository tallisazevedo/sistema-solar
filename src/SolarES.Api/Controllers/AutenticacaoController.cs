using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Identidade;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AutenticacaoController(AutenticacaoAppService servico) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.Login)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var resultado = await servico.LoginAsync(request.Email, request.Senha, ct);
        if (!resultado.Sucesso)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Credenciais invalidas.",
                Detail = "E-mail ou senha incorretos.",
            };
            problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;

            return new ObjectResult(problemDetails)
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                ContentTypes = { "application/problem+json" },
            };
        }

        return Ok(new LoginResponse(resultado.Token!, resultado.Usuario!.Nome, resultado.Usuario.Perfil));
    }
}
