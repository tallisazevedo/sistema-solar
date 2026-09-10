using Microsoft.AspNetCore.Mvc;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Api.Contratos;
using SolarES.Dominio.Tarifas;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/tarifas/municipios")]
public sealed class MunicipiosHspController(IRepositorioCrud<MunicipioHsp> repositorio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MunicipioHspResponse>>> Listar(CancellationToken ct)
    {
        var entidades = await repositorio.ListarAsync(ct);
        return Ok(entidades.Select(MunicipioHspResponse.DeEntidade).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MunicipioHspResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var entidade = await repositorio.ObterPorIdAsync(id, ct);
        return entidade is null ? NotFound() : Ok(MunicipioHspResponse.DeEntidade(entidade));
    }

    [HttpPost]
    public async Task<ActionResult<MunicipioHspResponse>> Criar(MunicipioHspRequest request, CancellationToken ct)
    {
        var entidade = request.ParaEntidade(Guid.NewGuid());
        repositorio.Adicionar(entidade);
        await repositorio.SalvarAlteracoesAsync(ct);

        var response = MunicipioHspResponse.DeEntidade(entidade);
        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, MunicipioHspRequest request, CancellationToken ct)
    {
        var existente = await repositorio.ObterPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        existente.CodigoIbge = request.CodigoIbge;
        existente.Nome = request.Nome;
        existente.Latitude = request.Latitude;
        existente.Longitude = request.Longitude;
        existente.DistanciaMarKm = request.DistanciaMarKm;
        existente.DistribuidoraId = request.DistribuidoraId;
        existente.HspPorMes = request.HspPorMes;
        existente.Fonte = request.Fonte;

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
