using Microsoft.EntityFrameworkCore;
using SolarES.Aplicacao.Leads;
using LeadEntidade = SolarES.Dominio.Lead.Lead;

namespace SolarES.Infraestrutura.Persistencia.Repositorios;

public sealed class EfLeadRepository(SolarESDbContext contexto) : ILeadRepository
{
    public Task<Guid?> ObterMunicipioIdPorCodigoAsync(string codigoIbge, CancellationToken ct) =>
        contexto.MunicipiosHsp.Where(m => m.CodigoIbge == codigoIbge).Select(m => (Guid?)m.Id).SingleOrDefaultAsync(ct);
    public void Adicionar(LeadEntidade lead) => contexto.Leads.Add(lead);
    public Task SalvarAlteracoesAsync(CancellationToken ct) => contexto.SaveChangesAsync(ct);
}
