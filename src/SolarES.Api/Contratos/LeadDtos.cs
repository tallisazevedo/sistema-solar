using SolarES.Dominio.Lead;
using SolarES.Dominio.Simulacao;

namespace SolarES.Api.Contratos;

public sealed record LeadResponse(Guid Id, string Nome, string Telefone, string Email,
    CanalPreferido? CanalPreferido, StatusLead Status, DateTimeOffset CriadoEm,
    bool RoteadoParaHumano, bool CalibracaoPendente, bool PossuiAnexo,
    Guid SimulacaoId, ResultadoSimulacao Resultado, IReadOnlyList<ConsentimentoResponse> Consentimentos);
public sealed record ConsentimentoResponse(FinalidadeConsentimento Finalidade,
    string VersaoTexto, DateTimeOffset ConcedidoEm);
