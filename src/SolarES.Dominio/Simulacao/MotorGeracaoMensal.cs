using SolarES.Dominio.Configuracao;

namespace SolarES.Dominio.Simulacao;

/// <summary>
/// Geracao mes a mes a partir do HSP sazonal do municipio (docs/02, secao "Geracao e
/// economia"). Funcao pura, mesmo padrao de MotorDimensionamento.
/// </summary>
public static class MotorGeracaoMensal
{
    public static IReadOnlyList<GeracaoMes> Calcular(
        EntradaSimulacao entrada,
        ConfiguracaoCalculo configuracao,
        IReadOnlyList<decimal> hspPorMes,
        decimal potenciaInstaladaKwp)
    {
        if (hspPorMes.Count != 12)
        {
            throw new ArgumentException("hspPorMes precisa ter exatamente 12 meses.", nameof(hspPorMes));
        }

        var custoDisponibilidade = configuracao.CustoDisponibilidadePorLigacao.Valor.ParaLigacao(entrada.TipoLigacao);
        var performanceRatio = configuracao.PerformanceRatio.Valor;
        var fatorOrientacao = configuracao.FatorOrientacaoPadrao.Valor;

        var resultado = new List<GeracaoMes>(12);
        for (var indice = 0; indice < 12; indice++)
        {
            var geracaoKwh = potenciaInstaladaKwp * hspPorMes[indice] * 30m * performanceRatio * fatorOrientacao;
            var consumoCompensavelKwh = Math.Max(0m, entrada.HistoricoConsumoKwh[indice] - custoDisponibilidade);
            var energiaCompensadaKwh = Math.Min(geracaoKwh, consumoCompensavelKwh);

            resultado.Add(new GeracaoMes(indice + 1, geracaoKwh, consumoCompensavelKwh, energiaCompensadaKwh));
        }

        return resultado;
    }
}
