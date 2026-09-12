using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Leads;
using SolarES.Dominio.Lead;
using LeadEntidade = SolarES.Dominio.Lead.Lead;

namespace SolarES.Infraestrutura.Persistencia.Repositorios;

public sealed class EfLeadRepository(SolarESDbContext contexto) : ILeadRepository
{
    public Task<Guid?> ObterMunicipioIdPorCodigoAsync(string codigoIbge, CancellationToken ct) =>
        contexto.MunicipiosHsp.Where(m => m.CodigoIbge == codigoIbge).Select(m => (Guid?)m.Id).SingleOrDefaultAsync(ct);
    public async Task<IReadOnlyList<LeadEntidade>> ListarDaLandingAsync(CancellationToken ct) =>
        await contexto.Leads.Where(l => l.Origem == OrigemLead.Landing)
            .OrderByDescending(l => l.CriadoEm).ToListAsync(ct);
    public async Task<IReadOnlyList<LeadEntidade>> ListarAsync(OrigemLead? origem, StatusLead? status,
        CancellationToken ct) => await contexto.Leads
        .Where(l => (origem == null || l.Origem == origem) && (status == null || l.Status == status))
        .OrderByDescending(l => l.CriadoEm).ToListAsync(ct);
    public Task<LeadEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct) =>
        contexto.Leads.SingleOrDefaultAsync(l => l.Id == id, ct);
    public Task<AnexoConta?> ObterAnexoAsync(Guid leadId, CancellationToken ct) =>
        contexto.AnexosConta.SingleOrDefaultAsync(a => a.LeadId == leadId, ct);
    public void AdicionarAnexo(AnexoConta anexo) => contexto.AnexosConta.Add(anexo);
    public async Task<IReadOnlyList<ConsentimentoLgpd>> ListarConsentimentosAsync(Guid leadId, CancellationToken ct) =>
        await contexto.ConsentimentosLgpd.Where(c => c.LeadId == leadId).OrderBy(c => c.ConcedidoEm).ToListAsync(ct);
    public void AdicionarConsentimento(ConsentimentoLgpd consentimento) => contexto.ConsentimentosLgpd.Add(consentimento);
    public void Adicionar(LeadEntidade lead) => contexto.Leads.Add(lead);
    public void AdicionarHistorico(HistoricoStatusLead historico) => contexto.HistoricosStatusLead.Add(historico);
    public Task SalvarAlteracoesAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
