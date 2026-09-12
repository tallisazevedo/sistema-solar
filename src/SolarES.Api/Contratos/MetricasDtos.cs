using SolarES.Dominio.Metricas;

namespace SolarES.Api.Contratos;

public sealed record RegistrarEventoFunilRequest(TipoEventoFunil Tipo, Guid SessaoFunilId);
public sealed record MetricasFunilResponse(int SessoesIniciadas, int SessoesConcluidas, decimal TaxaConclusaoPercentual);
