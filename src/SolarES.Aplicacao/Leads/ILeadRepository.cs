using LeadEntidade = SolarES.Dominio.Lead.Lead;

namespace SolarES.Aplicacao.Leads;

public interface ILeadRepository
{
    Task<Guid?> ObterMunicipioIdPorCodigoAsync(string codigoIbge, CancellationToken ct);
    void Adicionar(LeadEntidade lead);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
