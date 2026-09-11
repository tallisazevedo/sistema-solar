using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;

namespace SolarES.Api.Controllers;

[ApiController]
[AllowAnonymous]
[EnableCors("Landing")]
[Route("api/publico")]
public sealed class SimulacoesPublicasController(
    SimulacaoAppService simulacaoAppService,
    IRepositorioCrud<MunicipioHsp> municipiosRepositorio) : ControllerBase
{
    [HttpGet("municipios")]
    public async Task<ActionResult<IReadOnlyList<MunicipioPublicoResponse>>> ListarMunicipios(CancellationToken ct)
    {
        var municipios = await municipiosRepositorio.ListarAsync(ct);
        return Ok(municipios
            .OrderBy(municipio => municipio.Nome)
            .Select(municipio => new MunicipioPublicoResponse(municipio.CodigoIbge, municipio.Nome)));
    }

    [HttpPost("simulacoes")]
    public async Task<ActionResult<SimulacaoPublicaResponse>> Criar(
        CriarSimulacaoPublicaRequest request,
        CancellationToken ct)
    {
        var resultado = await simulacaoAppService.CriarPublicaAsync(ParaEntrada(request), ct);
        var response = ParaResponse(resultado);
        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    [HttpGet("simulacoes/{id:guid}")]
    public async Task<ActionResult<SimulacaoPublicaResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var resultado = await simulacaoAppService.ObterPublicaPorIdAsync(id, ct);
        return resultado is null ? NotFound() : Ok(ParaResponse(resultado));
    }

    private static EntradaSimulacao ParaEntrada(CriarSimulacaoPublicaRequest request) => new(
        Enumerable.Repeat(request.ConsumoMedioMensalKwh, 12).ToList(),
        request.TipoLigacao,
        request.PerfilImovel switch
        {
            PerfilImovel.Residencial => Subgrupo.B1,
            PerfilImovel.Rural => Subgrupo.B2,
            PerfilImovel.Comercial => Subgrupo.B3,
            _ => throw new ArgumentOutOfRangeException(nameof(request), "Perfil do imovel invalido."),
        },
        request.MunicipioCodigoIbge,
        request.TipoTelhado,
        request.AreaDisponivelM2,
        request.PossuiGeracaoPropria);

    private static SimulacaoPublicaResponse ParaResponse(SimulacaoPublicaResultado resultado)
    {
        var simulacao = resultado.Simulacao;
        return new(
            simulacao.Id,
            simulacao.PotenciaKwp,
            simulacao.QuantidadeModulos,
            simulacao.Capex,
            simulacao.EconomiaMensalAno1,
            simulacao.PaybackMeses,
            resultado.CalibracaoPendente);
    }
}
