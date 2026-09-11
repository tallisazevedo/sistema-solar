using SolarES.Dominio.Lead;
using SolarES.Dominio.Simulacao;

namespace SolarES.Api.Contratos;

public sealed record LeadResponse(Guid Id, string Nome, string Telefone, string Email,
    CanalPreferido? CanalPreferido, StatusLead Status, OrigemLead Origem,
    DateTimeOffset? VisitaTecnicaAgendadaPara, DateTimeOffset CriadoEm,
    bool RoteadoParaHumano, bool CalibracaoPendente, bool PossuiAnexo,
    Guid? SimulacaoId, ResultadoSimulacao? Resultado, IReadOnlyList<ConsentimentoResponse> Consentimentos,
    DateTimeOffset? ExpurgadoEm);
public sealed record ConsentimentoResponse(FinalidadeConsentimento Finalidade,
    string VersaoTexto, DateTimeOffset ConcedidoEm);
public sealed record CriarLeadManualRequest(string Nome, string Telefone, string Email, OrigemLead Origem,
    bool ConsentimentoContato, string VersaoTextoConsentimento);
public sealed record AlterarStatusLeadRequest(StatusLead Status, DateTimeOffset? VisitaTecnicaAgendadaPara);
