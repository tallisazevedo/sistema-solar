using Microsoft.AspNetCore.Mvc;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Propostas;

namespace SolarES.Api.Controllers;

[ApiController]
public sealed class EnviosPropostaController(EnvioPropostaAppService servico) : ControllerBase
{
    [HttpPost("api/propostas/{propostaId:guid}/envios")]
    public async Task<ActionResult<EnvioPropostaResponse>> Solicitar(Guid propostaId,
        SolicitarEnvioPropostaRequest request, CancellationToken ct)
    {
        try
        {
            var envio = await servico.SolicitarEnvioAsync(propostaId, request.Canal, request.Destino, ct);
            return CreatedAtAction(nameof(Listar), new { propostaId }, EnvioPropostaResponse.DeEntidade(envio));
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("api/propostas/{propostaId:guid}/envios")]
    public async Task<ActionResult<IReadOnlyList<EnvioPropostaResponse>>> Listar(Guid propostaId, CancellationToken ct) =>
        Ok((await servico.ListarEnviosAsync(propostaId, ct)).Select(EnvioPropostaResponse.DeEntidade));

    [HttpPost("api/propostas/{propostaId:guid}/envios/{envioId:guid}/reenvio")]
    public async Task<IActionResult> Reenviar(Guid propostaId, Guid envioId, CancellationToken ct) =>
        await servico.ReenviarAsync(envioId, ct) ? NoContent() : NotFound();
}
