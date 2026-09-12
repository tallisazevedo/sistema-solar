using Microsoft.AspNetCore.Mvc;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Proposta;

namespace SolarES.Api.Controllers;

[ApiController]
public sealed class PropostasController(PropostaAppService servico) : ControllerBase
{
    [HttpPost("api/simulacoes/{simulacaoId:guid}/proposta")]
    public async Task<ActionResult<PropostaResponse>> Gerar(Guid simulacaoId, CancellationToken ct)
    {
        var proposta = await servico.GerarAsync(simulacaoId, ct);
        var response = PropostaResponse.DeEntidade(proposta);
        return CreatedAtAction(nameof(ObterPdf), new { id = proposta.Id }, response);
    }

    [HttpGet("api/propostas")]
    public async Task<ActionResult<IReadOnlyList<PropostaResponse>>> Listar([FromQuery] StatusProposta? status, CancellationToken ct) =>
        Ok((await servico.ListarAsync(status, ct)).Select(PropostaResponse.DeEntidade));

    [HttpGet("api/propostas/{id:guid}")]
    public async Task<ActionResult<PropostaResponse>> Obter(Guid id, CancellationToken ct)
    {
        var proposta = await servico.ObterAsync(id, ct);
        return proposta is null ? NotFound() : Ok(PropostaResponse.DeEntidade(proposta));
    }

    [HttpGet("api/propostas/{id:guid}/pdf")]
    public async Task<IActionResult> ObterPdf(Guid id, CancellationToken ct)
    {
        var (conteudoPdf, numero) = await servico.ObterPdfAsync(id, ct);
        return File(conteudoPdf, "application/pdf", $"{numero}.pdf");
    }

    [HttpPost("api/propostas/{id:guid}/aceite")]
    public async Task<IActionResult> Aceitar(Guid id, CancellationToken ct) =>
        await servico.AceitarAsync(id, ct) ? NoContent() : NotFound();

    [HttpPost("api/propostas/{id:guid}/perda")]
    public async Task<IActionResult> MarcarPerdida(Guid id, [FromBody] MarcarPerdidaRequest? request, CancellationToken ct) =>
        await servico.MarcarPerdidaAsync(id, request?.Motivo, ct) ? NoContent() : NotFound();
}
