using System.ComponentModel.DataAnnotations;
using SolarES.Dominio.Simulacao;

namespace SolarES.Api.Contratos;

public enum PerfilImovel
{
    Residencial = 0,
    Rural = 1,
    Comercial = 2,
}

public sealed record CriarSimulacaoPublicaRequest(
    [Range(0.01, double.MaxValue)] decimal ConsumoMedioMensalKwh,
    TipoLigacao TipoLigacao,
    PerfilImovel PerfilImovel,
    [Required, MaxLength(7)] string MunicipioCodigoIbge,
    TipoTelhado TipoTelhado,
    [Range(0.01, double.MaxValue)] decimal AreaDisponivelM2,
    bool PossuiGeracaoPropria);

public sealed record MunicipioPublicoResponse(string CodigoIbge, string Nome);

public sealed record SimulacaoPublicaResponse(
    Guid Id,
    decimal PotenciaKwp,
    int QuantidadeModulos,
    decimal InvestimentoEstimado,
    decimal EconomiaMensalAno1,
    int? PaybackMeses,
    bool CalibracaoPendente);
