using Microsoft.AspNetCore.Mvc;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Api.Contratos;
using SolarES.Dominio.Precificacao;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/precificacao/faixas-preco")]
public sealed class FaixasPrecoController(IRepositorioCrud<FaixaPreco> repositorio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FaixaPrecoResponse>>> Listar(CancellationToken ct)
    {
        var entidades = await repositorio.ListarAsync(ct);
        return Ok(entidades.Select(FaixaPrecoResponse.DeEntidade).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FaixaPrecoResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var entidade = await repositorio.ObterPorIdAsync(id, ct);
        return entidade is null ? NotFound() : Ok(FaixaPrecoResponse.DeEntidade(entidade));
    }

    [HttpPost]
    public async Task<ActionResult<FaixaPrecoResponse>> Criar(FaixaPrecoRequest request, CancellationToken ct)
    {
        var entidade = request.ParaEntidade(Guid.NewGuid());
        repositorio.Adicionar(entidade);
        await repositorio.SalvarAlteracoesAsync(ct);

        var response = FaixaPrecoResponse.DeEntidade(entidade);
        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, FaixaPrecoRequest request, CancellationToken ct)
    {
        var existente = await repositorio.ObterPorIdAsync(id, ct);
        if (existente is null)
        {
            return NotFound();
        }

        existente.KwpMinimo = request.KwpMinimo;
        existente.KwpMaximo = request.KwpMaximo;
        existente.PrecoPorWp = request.PrecoPorWp;
        existente.TipoInstalacao = request.TipoInstalacao;
        existente.KitLitoral = request.KitLitoral;
        existente.Vigencia = request.Vigencia;

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
