using Microsoft.AspNetCore.Mvc;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Api.Contratos;
using SolarES.Dominio.Catalogo;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/catalogo/modulos-fotovoltaicos")]
public sealed class ModulosFotovoltaicosController(IRepositorioCrud<ModuloFotovoltaico> repositorio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ModuloFotovoltaicoResponse>>> Listar(CancellationToken ct)
    {
        var entidades = await repositorio.ListarAsync(ct);
        return Ok(entidades.Select(ModuloFotovoltaicoResponse.DeEntidade).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ModuloFotovoltaicoResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var entidade = await repositorio.ObterPorIdAsync(id, ct);
        return entidade is null ? NotFound() : Ok(ModuloFotovoltaicoResponse.DeEntidade(entidade));
    }

    [HttpPost]
    public async Task<ActionResult<ModuloFotovoltaicoResponse>> Criar(ModuloFotovoltaicoRequest request, CancellationToken ct)
    {
        var entidade = request.ParaEntidade(Guid.NewGuid());
        repositorio.Adicionar(entidade);
        await repositorio.SalvarAlteracoesAsync(ct);

        var response = ModuloFotovoltaicoResponse.DeEntidade(entidade);
        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, ModuloFotovoltaicoRequest request, CancellationToken ct)
    {
        var existente = await repositorio.ObterPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        existente.Fabricante = request.Fabricante;
        existente.Modelo = request.Modelo;
        existente.PotenciaW = request.PotenciaW;
        existente.LarguraMm = request.LarguraMm;
        existente.AlturaMm = request.AlturaMm;
        existente.EficienciaPercentual = request.EficienciaPercentual;
        existente.ResistenteNevoaSalina = request.ResistenteNevoaSalina;
        existente.Ativo = request.Ativo;

        await repositorio.SalvarAlteracoesAsync(ct);
        return NoContent();
    }

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
