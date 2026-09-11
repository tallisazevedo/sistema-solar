using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SolarES.Api.Contratos;
using SolarES.Aplicacao.Leads;
using SolarES.Dominio.Identidade;
using SolarES.Dominio.Lead;

namespace SolarES.Api.Controllers;

[ApiController]
[Route("api/leads")]
public sealed class LeadsController(ConsultaLeadsAppService servico, GerenciarLeadsAppService gerenciador,
    LeadAppService leadAppService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeadResponse>>> Listar([FromQuery] OrigemLead? origem,
        [FromQuery] StatusLead? status, CancellationToken ct) =>
        Ok((await servico.ListarAsync(origem, status, ct)).Select(ParaResponse));

    [HttpPost]
    [Authorize(Roles = "Dono,Vendedor")]
    public async Task<ActionResult<LeadResponse>> Criar(CriarLeadManualRequest request, CancellationToken ct)
    {
        var lead = await gerenciador.CriarManualAsync(request.Nome, request.Telefone, request.Email, request.Origem,
            request.ConsentimentoContato, request.VersaoTextoConsentimento, ct);
        var resultado = await servico.ObterAsync(lead.Id, ct) ?? throw new InvalidOperationException("Lead criado nao encontrado.");
        return CreatedAtAction(nameof(Obter), new { id = lead.Id }, ParaResponse(resultado));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Dono,Vendedor")]
    public async Task<IActionResult> AlterarStatus(Guid id, AlterarStatusLeadRequest request, CancellationToken ct)
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await gerenciador.AlterarStatusAsync(id, request.Status, request.VisitaTecnicaAgendadaPara,
            usuarioId, ct) ? NoContent() : NotFound();
    }

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

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpGet("{id:guid}/exportacao")]
    public async Task<ActionResult<ExportacaoLeadResponse>> Exportar(Guid id, CancellationToken ct)
    {
        var exportacao = await servico.ObterExportacaoAsync(id, ct);
        return exportacao is null ? NotFound() : Ok(ParaExportacaoResponse(exportacao));
    }

    [Authorize(Roles = nameof(PerfilUsuario.Dono))]
    [HttpPost("{id:guid}/eliminacao")]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct) =>
        await leadAppService.EliminarAsync(id, ct) ? NoContent() : NotFound();

    private static ExportacaoLeadResponse ParaExportacaoResponse(ExportacaoLeadResultado exportacao) => new(
        exportacao.Id, exportacao.Nome, exportacao.Telefone, exportacao.Email, exportacao.Status,
        exportacao.CriadoEm, exportacao.ExpurgadoEm, exportacao.SimulacaoId,
        exportacao.Consentimentos.Select(c => new ConsentimentoResponse(c.Finalidade, c.VersaoTexto, c.ConcedidoEm)).ToList(),
        exportacao.Anexo is null ? null : new AnexoMetadadoResponse(exportacao.Anexo.Tipo, exportacao.Anexo.Tamanho,
            exportacao.Anexo.RecebidoEm, exportacao.Anexo.DescartarAte, exportacao.Anexo.DescartadoEm));

    private static LeadResponse ParaResponse(LeadAdministrativoResultado lead) => new(
        lead.Id, lead.Nome, lead.Telefone, lead.Email, lead.CanalPreferido, lead.Status,
        lead.Origem, lead.VisitaTecnicaAgendadaPara, lead.CriadoEm,
        lead.RoteadoParaHumano, lead.CalibracaoPendente, lead.PossuiAnexo,
        lead.SimulacaoId, lead.Resultado,
        lead.Consentimentos.Select(c => new ConsentimentoResponse(c.Finalidade, c.VersaoTexto, c.ConcedidoEm)).ToList(),
        lead.ExpurgadoEm);
}
