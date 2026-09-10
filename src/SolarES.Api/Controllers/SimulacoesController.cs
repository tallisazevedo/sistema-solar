using Microsoft.AspNetCore.Mvc;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Simulacoes;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/simulacoes")]
public sealed class SimulacoesController(SimulacaoAppService servico) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SimulacaoDetalheResponse>> Criar(EntradaSimulacaoRequest request, CancellationToken ct)
    {
        var simulacao = await servico.CriarAsync(request.ParaEntrada(), ct);
        var response = SimulacaoDetalheResponse.DeEntidade(simulacao);
        return CreatedAtAction(nameof(ObterPorId), new { id = simulacao.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SimulacaoResumoResponse>>> Listar(CancellationToken ct)
    {
        var simulacoes = await servico.ListarAsync(ct);
        return Ok(simulacoes.Select(SimulacaoResumoResponse.DeEntidade).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SimulacaoDetalheResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var simulacao = await servico.ObterPorIdAsync(id, ct);
        return simulacao is null ? NotFound() : Ok(SimulacaoDetalheResponse.DeEntidade(simulacao));
    }
}
