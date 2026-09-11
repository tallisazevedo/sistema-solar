using LeadEntidade = SolarES.Dominio.Lead.Lead;

namespace SolarES.Aplicacao.Leads;

public interface ILeadRepository
{
    Task<Guid?> ObterMunicipioIdPorCodigoAsync(string codigoIbge, CancellationToken ct);
    Task<IReadOnlyList<LeadEntidade>> ListarDaLandingAsync(CancellationToken ct);
    Task<LeadEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct);
    void Adicionar(LeadEntidade lead);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
