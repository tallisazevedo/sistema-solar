using LeadEntidade = SolarES.Dominio.Lead.Lead;
using SolarES.Dominio.Lead;

namespace SolarES.Aplicacao.Leads;

public interface ILeadRepository
{
    Task<Guid?> ObterMunicipioIdPorCodigoAsync(string codigoIbge, CancellationToken ct);
    Task<IReadOnlyList<LeadEntidade>> ListarDaLandingAsync(CancellationToken ct);
    Task<LeadEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<AnexoConta?> ObterAnexoAsync(Guid leadId, CancellationToken ct);
    void AdicionarAnexo(AnexoConta anexo);
    Task<IReadOnlyList<ConsentimentoLgpd>> ListarConsentimentosAsync(Guid leadId, CancellationToken ct);
    void AdicionarConsentimento(ConsentimentoLgpd consentimento);
    void Adicionar(LeadEntidade lead);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}
