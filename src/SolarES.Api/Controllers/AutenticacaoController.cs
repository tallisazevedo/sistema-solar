using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Identidade;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AutenticacaoController(AutenticacaoAppService servico) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var resultado = await servico.LoginAsync(request.Email, request.Senha, ct);
        if (!resultado.Sucesso)
        {
            return Unauthorized();
        }

        return Ok(new LoginResponse(resultado.Token!, resultado.Usuario!.Nome, resultado.Usuario.Perfil));
    }
}
