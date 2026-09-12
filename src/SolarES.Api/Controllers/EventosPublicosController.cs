using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Metricas;
using SolarES.Dominio.Metricas;

namespace SolarES.Api.Controllers;

[ApiController, AllowAnonymous, EnableCors("Landing")]
[EnableRateLimiting(RateLimitingExtensions.PublicoEventos)]
[Route("api/publico/eventos")]
public sealed class EventosPublicosController(FunilAppService funil) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Registrar(RegistrarEventoFunilRequest request, CancellationToken ct)
    {
        if (request.Tipo != TipoEventoFunil.SimulacaoIniciada)
            return BadRequest("Somente o inicio da simulacao pode ser informado pelo cliente.");
        await funil.RegistrarInicioAsync(request.SessaoFunilId, ct);
        return Accepted();
    }
}
