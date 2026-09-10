using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using SolarES.Dominio.Simulacao;
using SolarES.Dominio.Tarifas;
using SimulacaoEntidade = SolarES.Dominio.Simulacao.Simulacao;

namespace SolarES.Api.Contratos;

public sealed record EntradaSimulacaoRequest(
    [Required, MinLength(12), MaxLength(12)] List<decimal> HistoricoConsumoKwh,
    TipoLigacao TipoLigacao,
    Subgrupo Subgrupo,
    [Required, MaxLength(7)] string MunicipioCodigoIbge,
    TipoTelhado TipoTelhado,
    [Range(0, double.MaxValue)] decimal AreaDisponivelM2,
    bool PossuiGeracaoPropria)
{
    public EntradaSimulacao ParaEntrada() => new(
        HistoricoConsumoKwh, TipoLigacao, Subgrupo, MunicipioCodigoIbge, TipoTelhado, AreaDisponivelM2, PossuiGeracaoPropria);
}

public sealed record SimulacaoResumoResponse(
    Guid Id,
    DateTimeOffset CriadoEm,
    decimal PotenciaKwp,
    int QuantidadeModulos,
    decimal Capex,
    decimal EconomiaMensalAno1,
    int? PaybackMeses,
    decimal? Tir,
    decimal Vpl,
    decimal CoberturaPercentual,
    bool RoteadaParaHumano,
    string? MotivoRoteamento)
{
    public static SimulacaoResumoResponse DeEntidade(SimulacaoEntidade s) => new(
        s.Id, s.CriadoEm, s.PotenciaKwp, s.QuantidadeModulos, s.Capex, s.EconomiaMensalAno1,
        s.PaybackMeses, s.Tir, s.Vpl, s.CoberturaPercentual, s.RoteadaParaHumano, s.MotivoRoteamento);
}

public sealed record SimulacaoDetalheResponse(
    Guid Id,
    DateTimeOffset CriadoEm,
    Guid ConfiguracaoVersaoId,
    ResultadoSimulacao Resultado)
{
    public static SimulacaoDetalheResponse DeEntidade(SimulacaoEntidade s) => new(
        s.Id,
        s.CriadoEm,
        s.ConfiguracaoVersaoId,
        JsonSerializer.Deserialize<ResultadoSimulacao>(s.ResultadoSnapshot)!);
}
