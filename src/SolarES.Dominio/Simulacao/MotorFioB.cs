using SolarES.Dominio.Configuracao;

namespace SolarES.Dominio.Simulacao;

/// <summary>
/// Custo do Fio B (Lei 14.300/2022, art. 27) e economia liquida mes a mes (docs/02,
/// secao "Geracao e economia"). Funcao pura, mesmo padrao dos demais motores.
///
/// Atencao ao ler/alterar este arquivo: o percentual incide sobre o componente Fio B
/// (valorFioBPorKwh), NUNCA sobre a tarifa cheia. E a armadilha nº 1 do docs/02.
/// </summary>
public static class MotorFioB
{
    public static decimal ResolverPercentualAno(ConfiguracaoCalculo configuracao, int ano)
    {
        var cronograma = configuracao.CronogramaFioB.Valor;
        var doAno = cronograma.FirstOrDefault(p => p.Ano == ano);
        if (doAno is not null)
        {
            return doAno.Percentual;
        }

        return configuracao.EstrategiaFioBForaCronograma.Valor switch
        {
            EstrategiaFioBForaCronograma.MantemUltimoPercentual => ano > cronograma.Max(p => p.Ano)
                ? cronograma.OrderByDescending(p => p.Ano).First().Percentual
                : cronograma.OrderBy(p => p.Ano).First().Percentual,
            var estrategia => throw new NotSupportedException($"Estrategia de fallback do Fio B nao implementada: {estrategia}"),
        };
    }

    public static IReadOnlyList<EconomiaMes> Calcular(
        IReadOnlyList<GeracaoMes> geracaoMensal,
        ConfiguracaoCalculo configuracao,
        int ano,
        decimal valorFioBPorKwh,
        decimal tarifaCheia)
    {
        var percentual = ResolverPercentualAno(configuracao, ano);

        return geracaoMensal
            .Select(mes =>
            {
                var custoFioB = mes.EnergiaCompensadaKwh * valorFioBPorKwh * percentual;
                var economiaBruta = mes.EnergiaCompensadaKwh * tarifaCheia;
                return new EconomiaMes(mes.Mes, custoFioB, economiaBruta, economiaBruta - custoFioB);
            })
            .ToList();
    }
}
