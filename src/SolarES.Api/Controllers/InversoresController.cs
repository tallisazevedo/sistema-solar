using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Api.Contratos;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Identidade;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/catalogo/inversores")]
public sealed class InversoresController(IRepositorioCrud<Inversor> repositorio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InversorResponse>>> Listar(CancellationToken ct)
    {
        var entidades = await repositorio.ListarAsync(ct);
        return Ok(entidades.Select(InversorResponse.DeEntidade).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InversorResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var entidade = await repositorio.ObterPorIdAsync(id, ct);
        return entidade is null ? NotFound() : Ok(InversorResponse.DeEntidade(entidade));
    }

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpPost]
    public async Task<ActionResult<InversorResponse>> Criar(InversorRequest request, CancellationToken ct)
    {
        var entidade = request.ParaEntidade(Guid.NewGuid());
        repositorio.Adicionar(entidade);
        await repositorio.SalvarAlteracoesAsync(ct);

        var response = InversorResponse.DeEntidade(entidade);
        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, response);
    }

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, InversorRequest request, CancellationToken ct)
    {
        var existente = await repositorio.ObterPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        existente.Fabricante = request.Fabricante;
        existente.Modelo = request.Modelo;
        existente.PotenciaW = request.PotenciaW;
        existente.QuantidadeMppt = request.QuantidadeMppt;
        existente.Tipo = request.Tipo;
        existente.Ativo = request.Ativo;

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
