using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Metricas;

namespace SolarES.Api.Controllers;

[ApiController, Route("api/metricas")]
[Authorize(Roles = "Dono")]
public sealed class MetricasController(FunilAppService funil) : ControllerBase
{
    [HttpGet("funil")]
    public async Task<ActionResult<MetricasFunilResponse>> ObterFunil(DateTimeOffset de, DateTimeOffset ate,
        CancellationToken ct)
    {
        var resultado = await funil.ObterAsync(de, ate, ct);
        return Ok(new MetricasFunilResponse(resultado.Funil.SessoesIniciadas, resultado.Funil.SessoesConcluidas,
            resultado.Funil.TaxaConclusaoPercentual, resultado.Funil.LeadsCapturados, resultado.Funil.AnexosOferecidos,
            resultado.AnexoLanding.LeadsLanding, resultado.AnexoLanding.LeadsComAnexo,
            resultado.AnexoLanding.PercentualComAnexo,
            resultado.ConversaoPorOrigem.Select(c => new ConversaoOrigemResponse(c.Origem, c.LeadsCriados,
                c.LeadsConvertidos, c.ConversaoPercentual)).ToList()));
    }
}
