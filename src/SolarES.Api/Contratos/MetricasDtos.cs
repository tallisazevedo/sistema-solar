using SolarES.Dominio.Lead;
using SolarES.Dominio.Metricas;

namespace SolarES.Api.Contratos;

public sealed record RegistrarEventoFunilRequest(TipoEventoFunil Tipo, Guid SessaoFunilId);

public sealed record MetricasFunilResponse(int SessoesIniciadas, int SessoesConcluidas,
    decimal TaxaConclusaoPercentual, int LeadsCapturados, int AnexosOferecidos,
    int LeadsLanding, int LeadsComAnexo, decimal PercentualComAnexo,
    IReadOnlyList<ConversaoOrigemResponse> ConversaoPorOrigem);

public sealed record ConversaoOrigemResponse(OrigemLead Origem, int LeadsCriados, int LeadsConvertidos,
    decimal ConversaoPercentual);
