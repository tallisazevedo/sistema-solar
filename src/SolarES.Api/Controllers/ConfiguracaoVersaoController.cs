using Microsoft.AspNetCore.Mvc;
using SolarES.Aplicacao.Configuracao;
using SolarES.Api.Contratos;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/configuracao")]
public sealed class ConfiguracaoVersaoController(ConfiguracaoVersaoAppService servico, IConfiguracaoVersaoRepository repositorio) : ControllerBase
{
    [HttpGet("ativa")]
    public async Task<ActionResult<ConfiguracaoVersaoResponse>> ObterAtiva(CancellationToken ct)
    {
        var versao = await servico.ObterVersaoAtivaAsync(ct);
        return versao is null ? NotFound() : Ok(ConfiguracaoVersaoResponse.DeEntidade(versao));
    }

    [HttpGet("rascunho")]
    public async Task<ActionResult<ConfiguracaoVersaoResponse>> ObterRascunho(CancellationToken ct)
    {
        var versao = await repositorio.ObterRascunhoAsync(ct);
        return versao is null ? NotFound() : Ok(ConfiguracaoVersaoResponse.DeEntidade(versao));
    }

    [HttpPost("rascunhos")]
    public async Task<ActionResult<ConfiguracaoVersaoResponse>> CriarRascunho(CriarRascunhoRequest request, CancellationToken ct)
    {
        var versao = await servico.CriarRascunhoAsync(request.Payload, request.Observacao, ct);
        var response = ConfiguracaoVersaoResponse.DeEntidade(versao);
        return CreatedAtAction(nameof(ObterRascunho), null, response);
    }

    [HttpPut("rascunhos/{id:guid}/payload")]
    public async Task<IActionResult> AtualizarPayload(Guid id, ConfiguracaoCalculoRequest request, CancellationToken ct)
    {
        var versao = await repositorio.ObterPorIdAsync(id, ct);
        if (versao is null)
        {
            return NotFound();
        }

        versao.AtualizarPayload(request.Payload);
        await repositorio.SalvarAlteracoesAsync(ct);
        return NoContent();
    }

    [HttpPost("rascunhos/{id:guid}/publicar")]
    public async Task<IActionResult> Publicar(Guid id, PublicarRascunhoRequest request, CancellationToken ct)
    {
        await servico.PublicarAsync(id, request.UsuarioId, ct);
        return NoContent();
    }
}
