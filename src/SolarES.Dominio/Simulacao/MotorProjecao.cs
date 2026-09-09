using SolarES.Dominio.Configuracao;

namespace SolarES.Dominio.Simulacao;

/// <summary>
/// Projecao do horizonte configurado (docs/02, secao "Financeiro, horizonte de 25
/// anos"), aplicando degradacao do modulo e inflacao tarifaria ano a ano sobre a
/// geracao mensal do ano 1 (T09) e o Fio B do ano-calendario (T10). Funcao pura,
/// mesmo padrao dos demais motores.
/// </summary>
public static class MotorProjecao
{
    public static IReadOnlyList<AnoProjecao> Projetar(
        IReadOnlyList<GeracaoMes> geracaoMensalAno1,
        ConfiguracaoCalculo configuracao,
        int anoCalendarioInicial,
        decimal tarifaCheiaAno1,
        decimal valorFioBPorKwhAno1)
    {
        var degradacaoAnual = configuracao.DegradacaoAnual.Valor;
        var inflacaoTarifaria = configuracao.InflacaoTarifaria.Valor;
        var horizonteAnos = configuracao.HorizonteAnos.Valor;

        var resultado = new List<AnoProjecao>(horizonteAnos);

        for (var ano = 1; ano <= horizonteAnos; ano++)
        {
            var fatorDegradacao = Potencia(1m - degradacaoAnual, ano - 1);
            var fatorInflacao = Potencia(1m + inflacaoTarifaria, ano - 1);
            var anoCalendario = anoCalendarioInicial + (ano - 1);
            var percentualFioB = MotorFioB.ResolverPercentualAno(configuracao, anoCalendario);

            decimal geracaoAnualKwh = 0m;
            decimal economiaLiquidaAnualReais = 0m;

            foreach (var mes in geracaoMensalAno1)
            {
                var geracaoDegradadaKwh = mes.GeracaoKwh * fatorDegradacao;
                var energiaCompensadaKwh = Math.Min(geracaoDegradadaKwh, mes.ConsumoCompensavelKwh);

                var tarifaCheiaAno = tarifaCheiaAno1 * fatorInflacao;
                var valorFioBPorKwhAno = valorFioBPorKwhAno1 * fatorInflacao;
                var custoFioB = energiaCompensadaKwh * valorFioBPorKwhAno * percentualFioB;
                var economiaBruta = energiaCompensadaKwh * tarifaCheiaAno;

                geracaoAnualKwh += geracaoDegradadaKwh;
                economiaLiquidaAnualReais += economiaBruta - custoFioB;
            }

            var economiaLiquidaAnualEmTermosReaisReais = economiaLiquidaAnualReais / fatorInflacao;

            resultado.Add(new AnoProjecao(ano, anoCalendario, geracaoAnualKwh, economiaLiquidaAnualReais, economiaLiquidaAnualEmTermosReaisReais));
        }

        return resultado;
    }

    private static decimal Potencia(decimal @base, int expoente)
    {
        var resultado = 1m;
        for (var i = 0; i < expoente; i++)
        {
            resultado *= @base;
        }

        return resultado;
    }
}
