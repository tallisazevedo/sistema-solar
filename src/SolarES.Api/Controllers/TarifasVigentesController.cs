using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Api.Contratos;
using SolarES.Dominio.Identidade;
using SolarES.Dominio.Tarifas;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/tarifas/vigentes")]
public sealed class TarifasVigentesController(IRepositorioCrud<TarifaVigente> repositorio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TarifaVigenteResponse>>> Listar(CancellationToken ct)
    {
        var entidades = await repositorio.ListarAsync(ct);
        return Ok(entidades.Select(TarifaVigenteResponse.DeEntidade).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TarifaVigenteResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var entidade = await repositorio.ObterPorIdAsync(id, ct);
        return entidade is null ? NotFound() : Ok(TarifaVigenteResponse.DeEntidade(entidade));
    }

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpPost]
    public async Task<ActionResult<TarifaVigenteResponse>> Criar(TarifaVigenteRequest request, CancellationToken ct)
    {
        var entidade = request.ParaEntidade(Guid.NewGuid());
        repositorio.Adicionar(entidade);
        await repositorio.SalvarAlteracoesAsync(ct);

        var response = TarifaVigenteResponse.DeEntidade(entidade);
        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, response);
    }

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, TarifaVigenteRequest request, CancellationToken ct)
    {
        var existente = await repositorio.ObterPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        existente.DistribuidoraId = request.DistribuidoraId;
        existente.Subgrupo = request.Subgrupo;
        existente.TarifaTe = request.TarifaTe;
        existente.TarifaTusd = request.TarifaTusd;
        existente.ValorFioBPorKwh = request.ValorFioBPorKwh;
        existente.AliquotaIcms = request.AliquotaIcms;
        existente.AliquotaPisCofins = request.AliquotaPisCofins;
        existente.VigenciaInicio = request.VigenciaInicio;
        existente.VigenciaFim = request.VigenciaFim;
        existente.ResolucaoHomologatoria = request.ResolucaoHomologatoria;
        existente.Fonte = request.Fonte;

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
