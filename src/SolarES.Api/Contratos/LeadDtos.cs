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
public sealed record ExportacaoLeadResponse(Guid Id, string Nome, string Telefone, string Email,
    StatusLead Status, DateTimeOffset CriadoEm, DateTimeOffset? ExpurgadoEm, Guid? SimulacaoId,
    IReadOnlyList<ConsentimentoResponse> Consentimentos, AnexoMetadadoResponse? Anexo);
public sealed record AnexoMetadadoResponse(TipoAnexoConta Tipo, long Tamanho, DateTimeOffset RecebidoEm,
    DateTimeOffset DescartarAte, DateTimeOffset? DescartadoEm);
