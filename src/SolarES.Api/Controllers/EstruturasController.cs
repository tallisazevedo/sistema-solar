using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Api.Contratos;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Identidade;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/catalogo/estruturas")]
public sealed class EstruturasController(IRepositorioCrud<Estrutura> repositorio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EstruturaResponse>>> Listar(CancellationToken ct)
    {
        var entidades = await repositorio.ListarAsync(ct);
        return Ok(entidades.Select(EstruturaResponse.DeEntidade).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EstruturaResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var entidade = await repositorio.ObterPorIdAsync(id, ct);
        return entidade is null ? NotFound() : Ok(EstruturaResponse.DeEntidade(entidade));
    }

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpPost]
    public async Task<ActionResult<EstruturaResponse>> Criar(EstruturaRequest request, CancellationToken ct)
    {
        var entidade = request.ParaEntidade(Guid.NewGuid());
        repositorio.Adicionar(entidade);
        await repositorio.SalvarAlteracoesAsync(ct);

        var response = EstruturaResponse.DeEntidade(entidade);
        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, response);
    }

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, EstruturaRequest request, CancellationToken ct)
    {
        var existente = await repositorio.ObterPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        existente.Descricao = request.Descricao;
        existente.TipoTelhado = request.TipoTelhado;
        existente.ResistenteNevoaSalina = request.ResistenteNevoaSalina;
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
