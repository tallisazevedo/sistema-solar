using LeadEntidade = SolarES.Dominio.Lead.Lead;
using SolarES.Dominio.Lead;

namespace SolarES.Aplicacao.Leads;

public interface ILeadRepository
{
    Task<Guid?> ObterMunicipioIdPorCodigoAsync(string codigoIbge, CancellationToken ct);
    Task<IReadOnlyList<LeadEntidade>> ListarDaLandingAsync(CancellationToken ct);
    Task<IReadOnlyList<LeadEntidade>> ListarAsync(OrigemLead? origem, StatusLead? status, CancellationToken ct);
    Task<LeadEntidade?> ObterPorIdAsync(Guid id, CancellationToken ct);
    /// <summary>Quantos leads com o mesmo e-mail ou telefone (normalizados) ja foram criados desde o momento informado -- guarda anti-spam da issue #33.</summary>
    Task<int> ContarPorContatoDesdeAsync(string emailNormalizado, string telefoneNormalizado, DateTimeOffset desde, CancellationToken ct);
    Task<AnexoConta?> ObterAnexoAsync(Guid leadId, CancellationToken ct);
    Task<IReadOnlyList<AnexoConta>> ListarAnexosParaDescarteAsync(DateTimeOffset ate, CancellationToken ct);
    void AdicionarAnexo(AnexoConta anexo);
    Task<IReadOnlyList<ConsentimentoLgpd>> ListarConsentimentosAsync(Guid leadId, CancellationToken ct);
    void AdicionarConsentimento(ConsentimentoLgpd consentimento);
    void Adicionar(LeadEntidade lead);
    void AdicionarHistorico(HistoricoStatusLead historico);
    Task<MetricasAnexoLandingResultado> ObterMetricasAnexoLandingAsync(DateTimeOffset de, DateTimeOffset ate,
        CancellationToken ct);
    Task<IReadOnlyList<ConversaoOrigemResultado>> ObterConversaoPorOrigemAsync(DateTimeOffset de,
        DateTimeOffset ate, CancellationToken ct);
    Task SalvarAlteracoesAsync(CancellationToken ct);
}

public sealed record MetricasAnexoLandingResultado(int LeadsLanding, int LeadsComAnexo)
{
    public decimal PercentualComAnexo => LeadsLanding == 0
        ? 0 : decimal.Round(LeadsComAnexo * 100m / LeadsLanding, 2);
}

public sealed record ConversaoOrigemResultado(OrigemLead Origem, int LeadsCriados, int LeadsConvertidos)
{
    public decimal ConversaoPercentual => LeadsCriados == 0
        ? 0 : decimal.Round(LeadsConvertidos * 100m / LeadsCriados, 2);
}
