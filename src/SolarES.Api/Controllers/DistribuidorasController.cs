using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Api.Contratos;
using SolarES.Dominio.Identidade;
using SolarES.Dominio.Tarifas;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/tarifas/distribuidoras")]
public sealed class DistribuidorasController(IRepositorioCrud<Distribuidora> repositorio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DistribuidoraResponse>>> Listar(CancellationToken ct)
    {
        var entidades = await repositorio.ListarAsync(ct);
        return Ok(entidades.Select(DistribuidoraResponse.DeEntidade).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DistribuidoraResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var entidade = await repositorio.ObterPorIdAsync(id, ct);
        return entidade is null ? NotFound() : Ok(DistribuidoraResponse.DeEntidade(entidade));
    }

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpPost]
    public async Task<ActionResult<DistribuidoraResponse>> Criar(DistribuidoraRequest request, CancellationToken ct)
    {
        var entidade = request.ParaEntidade(Guid.NewGuid());
        repositorio.Adicionar(entidade);
        await repositorio.SalvarAlteracoesAsync(ct);

        var response = DistribuidoraResponse.DeEntidade(entidade);
        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, response);
    }

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, DistribuidoraRequest request, CancellationToken ct)
    {
        var existente = await repositorio.ObterPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        existente.Nome = request.Nome;
        existente.SiglaAneel = request.SiglaAneel;
        existente.Ativa = request.Ativa;

        await repositorio.SalvarAlteracoesAsync(ct);
        return NoContent();
    }

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        var existente = await repositorio.ObterPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        repositorio.Remover(existente);
        await repositorio.SalvarAlteracoesAsync(ct);
        return NoContent();
    }
}
