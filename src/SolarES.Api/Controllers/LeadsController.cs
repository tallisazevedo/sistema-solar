using Microsoft.AspNetCore.Mvc;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Leads;
using SolarES.Dominio.Lead;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/leads")]
public sealed class LeadsController(ConsultaLeadsAppService servico) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeadResponse>>> Listar(CancellationToken ct) =>
        Ok((await servico.ListarAsync(ct)).Select(ParaResponse));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeadResponse>> Obter(Guid id, CancellationToken ct)
    {
        var lead = await servico.ObterAsync(id, ct);
        return lead is null ? NotFound() : Ok(ParaResponse(lead));
    }

    [HttpGet("{id:guid}/anexo")]
    public async Task<IActionResult> BaixarAnexo(Guid id, CancellationToken ct)
    {
        var anexo = await servico.ObterAnexoAsync(id, ct);
        if (anexo is null) return NotFound();
        var (tipo, extensao) = anexo.Tipo switch
        {
            TipoAnexoConta.Pdf => ("application/pdf", "pdf"),
            TipoAnexoConta.Jpeg => ("image/jpeg", "jpg"),
            _ => ("image/png", "png"),
        };
        return File(anexo.Conteudo, tipo, $"conta.{extensao}");
    }

    private static LeadResponse ParaResponse(LeadAdministrativoResultado lead) => new(
        lead.Id, lead.Nome, lead.Telefone, lead.Email, lead.CanalPreferido, lead.Status,
        lead.CriadoEm, lead.RoteadoParaHumano, lead.CalibracaoPendente, lead.PossuiAnexo,
        lead.SimulacaoId, lead.Resultado);
}
