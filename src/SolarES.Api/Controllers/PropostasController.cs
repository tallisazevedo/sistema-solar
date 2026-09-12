using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
        var responsavelUsuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var proposta = await servico.GerarAsync(simulacaoId, responsavelUsuarioId, ct);
        var calibracaoPendente = await servico.PossuiCalibracaoPendenteAsync(proposta, ct);
        var response = PropostaResponse.DeEntidade(proposta, calibracaoPendente);
        return CreatedAtAction(nameof(ObterPdf), new { id = proposta.Id }, response);
    }

    [HttpGet("api/propostas")]
    public async Task<ActionResult<IReadOnlyList<PropostaResponse>>> Listar([FromQuery] StatusProposta? status, CancellationToken ct)
    {
        var propostas = await servico.ListarAsync(status, ct);
        var respostas = new List<PropostaResponse>(propostas.Count);
        foreach (var proposta in propostas)
        {
            respostas.Add(PropostaResponse.DeEntidade(proposta, await servico.PossuiCalibracaoPendenteAsync(proposta, ct)));
        }
        return Ok(respostas);
    }

    [HttpGet("api/propostas/{id:guid}")]
    public async Task<ActionResult<PropostaResponse>> Obter(Guid id, CancellationToken ct)
    {
        var proposta = await servico.ObterAsync(id, ct);
        if (proposta is null) return NotFound();
        return Ok(PropostaResponse.DeEntidade(proposta, await servico.PossuiCalibracaoPendenteAsync(proposta, ct)));
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

    [HttpPost("api/propostas/{id:guid}/renovacao")]
    [Authorize(Roles = "Dono,Vendedor")]
    public async Task<ActionResult<PropostaResponse>> Renovar(Guid id, CancellationToken ct)
    {
        try
        {
            var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var proposta = await servico.RenovarAsync(id, usuarioId, ct);
            return Ok(PropostaResponse.DeEntidade(proposta,
                await servico.PossuiCalibracaoPendenteAsync(proposta, ct)));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
