using SolarES.Dominio.Lead;

namespace SolarES.Api.Contratos;

public sealed record CapturarLeadPublicoRequest(string Nome, string Telefone, string Email,
    CanalPreferido CanalPreferido, IReadOnlyList<FinalidadeConsentimento> FinalidadesAceitas,
    string VersaoTexto, IFormFile? Anexo);
public sealed record CapturarLeadPublicoResponse(string Desfecho);
