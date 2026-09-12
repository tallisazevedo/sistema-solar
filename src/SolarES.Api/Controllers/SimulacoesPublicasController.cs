using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Text.Json;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Compartilhado;
using SolarES.Aplicacao.Simulacoes;
using SolarES.Aplicacao.Leads;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;
using SolarES.Aplicacao.Metricas;

namespace SolarES.Api.Controllers;

[ApiController]
[AllowAnonymous]
[EnableCors("Landing")]
[EnableRateLimiting(RateLimitingExtensions.PublicoLeitura)]
[Route("api/publico")]
public sealed class SimulacoesPublicasController(
    SimulacaoAppService simulacaoAppService,
    LeadAppService leadAppService,
    FunilAppService funilAppService,
    IRepositorioCrud<MunicipioHsp> municipiosRepositorio,
    IConfiguration configuracao) : ControllerBase
{
    [HttpPost("simulacoes/{id:guid}/lead")]
    [EnableRateLimiting(RateLimitingExtensions.PublicoLead)]
    public async Task<ActionResult<CapturarLeadPublicoResponse>> CapturarLead(Guid id,
        [FromForm] CapturarLeadPublicoRequest request, CancellationToken ct)
    {
        try
        {
            byte[]? conteudoAnexo = null;
            if (request.Anexo is not null)
            {
                var tamanhoMaximoAnexo = configuracao.GetValue<long?>("AnexosConta:TamanhoMaximoBytes") ?? 10_000_000;
                if (request.Anexo.Length > tamanhoMaximoAnexo)
                {
                    // Nunca copia o conteudo pro armazenamento -- o anexo grande
                    // demais e' recusado antes de qualquer efeito.
                    var limiteMb = Math.Round(tamanhoMaximoAnexo / 1_000_000.0, 1);
                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status413PayloadTooLarge,
                        Title = "Anexo grande demais.",
                        Detail = $"O arquivo ultrapassa o limite de {limiteMb} MB. Formatos aceitos: PDF, JPG e PNG.",
                    };
                    problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;
                    return new ObjectResult(problemDetails)
                    {
                        StatusCode = StatusCodes.Status413PayloadTooLarge,
                        ContentTypes = { "application/problem+json" },
                    };
                }

                await using var memoria = new MemoryStream();
                await request.Anexo.CopyToAsync(memoria, ct);
                conteudoAnexo = memoria.ToArray();
            }
            var desfecho = await leadAppService.CapturarPublicoAsync(id, request.Nome, request.Telefone,
                request.Email, request.CanalPreferido, request.FinalidadesAceitas,
                request.VersaoTexto, conteudoAnexo, request.SessaoFunilId, ct);
            return Ok(new CapturarLeadPublicoResponse(desfecho.ToString()));
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("municipios")]
    public async Task<ActionResult<IReadOnlyList<MunicipioPublicoResponse>>> ListarMunicipios(CancellationToken ct)
    {
        var municipios = await municipiosRepositorio.ListarAsync(ct);
        return Ok(municipios
            .OrderBy(municipio => municipio.Nome)
            .Select(municipio => new MunicipioPublicoResponse(municipio.CodigoIbge, municipio.Nome)));
    }

    [HttpPost("simulacoes")]
    [EnableRateLimiting(RateLimitingExtensions.PublicoSimulacao)]
    public async Task<ActionResult<SimulacaoPublicaResponse>> Criar(
        CriarSimulacaoPublicaRequest request,
        CancellationToken ct)
    {
        var historico = ObterHistorico(request);
        if (historico is null)
        {
            ModelState.AddModelError(nameof(request.HistoricoConsumoKwh),
                "Informe o consumo médio ou exatamente os 12 consumos mensais, todos maiores que zero.");
            return ValidationProblem(ModelState);
        }
        var resultado = await simulacaoAppService.CriarPublicaAsync(ParaEntrada(request, historico), ct);
        if (request.SessaoFunilId is { } sessaoFunilId)
            await funilAppService.RegistrarConclusaoAsync(sessaoFunilId, resultado.Simulacao.Id, ct);
        var response = ParaResponse(resultado);
        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    [HttpGet("simulacoes/{id:guid}")]
    public async Task<ActionResult<SimulacaoPublicaResponse>> ObterPorId(Guid id, CancellationToken ct)
    {
        var resultado = await simulacaoAppService.ObterPublicaPorIdAsync(id, ct);
        return resultado is null ? NotFound() : Ok(ParaResponse(resultado));
    }

    private static IReadOnlyList<decimal>? ObterHistorico(CriarSimulacaoPublicaRequest request)
    {
        if (request.HistoricoConsumoKwh is { Count: 12 } mensal && mensal.All(valor => valor > 0))
            return mensal;
        if (request.HistoricoConsumoKwh is null && request.ConsumoMedioMensalKwh is > 0)
            return Enumerable.Repeat(request.ConsumoMedioMensalKwh.Value, 12).ToList();
        return null;
    }

    private static EntradaSimulacao ParaEntrada(CriarSimulacaoPublicaRequest request, IReadOnlyList<decimal> historico) => new(
        historico,
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
        var detalhe = JsonSerializer.Deserialize<ResultadoSimulacao>(simulacao.ResultadoSnapshot)
            ?? throw new InvalidOperationException("Resultado da simulacao invalido.");
        var ocultaNumeros = simulacao.RoteadaParaHumano;
        return new(
            simulacao.Id,
            ocultaNumeros ? null : simulacao.PotenciaKwp,
            ocultaNumeros ? null : simulacao.QuantidadeModulos,
            ocultaNumeros ? null : simulacao.Capex,
            ocultaNumeros ? null : simulacao.EconomiaMensalAno1,
            ocultaNumeros ? null : simulacao.PaybackMeses,
            resultado.CalibracaoPendente,
            ocultaNumeros ? null : simulacao.CoberturaPercentual,
            detalhe.KitLitoral,
            detalhe.InstalacaoRecomendada,
            simulacao.RoteadaParaHumano,
            simulacao.MotivoRoteamento,
            ocultaNumeros ? null : detalhe.Projecao);
    }
}
