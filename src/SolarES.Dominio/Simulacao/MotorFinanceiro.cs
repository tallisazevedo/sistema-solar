namespace SolarES.Dominio.Simulacao;

/// <summary>
/// Payback simples e descontado, VPL e TIR sobre o fluxo de caixa anual da T11
/// (docs/02, secao "Financeiro, horizonte de 25 anos"). Funcao pura, mesmo padrao
/// dos demais motores.
///
/// TIR: busca por bisseccao em double sobre o intervalo [-99%, 1000%] -- unico
/// lugar do motor onde double e aceitavel (.claude/rules/dominio.md), porque
/// bisseccao precisa de aritmetica de ponto flutuante barata em loop; o resultado
/// final volta para decimal. Convergencia: para quando |VPL(taxa)| &lt; 1e-9 ou apos
/// 200 iteracoes (o que vier primeiro). Sem raiz garantida no intervalo (VPL tem o
/// mesmo sinal nos dois extremos) retorna null em vez de um numero sem sentido.
/// </summary>
public static class MotorFinanceiro
{
    private const double TaxaMinima = -0.99;
    private const double TaxaMaxima = 10.0;
    private const double ToleranciaVpl = 1e-9;
    private const int MaxIteracoesBisseccao = 200;

    public static ResultadoFinanceiro Calcular(IReadOnlyList<decimal> fluxosAnuais, decimal capex, decimal taxaDesconto)
    {
        var paybackSimples = CalcularPaybackMeses(fluxosAnuais, capex);

        var fluxosDescontados = fluxosAnuais
            .Select((fluxo, indice) => fluxo / MatematicaFinanceira.Potencia(1m + taxaDesconto, indice + 1))
            .ToList();
        var paybackDescontado = CalcularPaybackMeses(fluxosDescontados, capex);

        var vpl = CalcularVpl(fluxosAnuais, capex, taxaDesconto);
        var tir = CalcularTir(fluxosAnuais, capex);

        return new ResultadoFinanceiro(paybackSimples, paybackDescontado, vpl, tir);
    }

    private static int? CalcularPaybackMeses(IReadOnlyList<decimal> fluxos, decimal capex)
    {
        var acumulado = 0m;
        for (var indice = 0; indice < fluxos.Count; indice++)
        {
            var acumuladoAntes = acumulado;
            acumulado += fluxos[indice];

            if (acumulado < capex)
            {
                continue;
            }

            var faltava = capex - acumuladoAntes;
            var mesesNoAno = fluxos[indice] == 0m
                ? 12
                : Math.Clamp((int)Math.Ceiling(faltava / fluxos[indice] * 12m), 1, 12);

            return indice * 12 + mesesNoAno;
        }

        return null;
    }

    private static decimal CalcularVpl(IReadOnlyList<decimal> fluxosAnuais, decimal capex, decimal taxaDesconto)
    {
        var vpl = -capex;
        for (var indice = 0; indice < fluxosAnuais.Count; indice++)
        {
            vpl += fluxosAnuais[indice] / MatematicaFinanceira.Potencia(1m + taxaDesconto, indice + 1);
        }

        return vpl;
    }

    private static decimal? CalcularTir(IReadOnlyList<decimal> fluxosAnuais, decimal capex)
    {
        double VplEm(double taxa)
        {
            var vpl = -(double)capex;
            for (var indice = 0; indice < fluxosAnuais.Count; indice++)
            {
                vpl += (double)fluxosAnuais[indice] / Math.Pow(1 + taxa, indice + 1);
            }

            return vpl;
        }

        var inferior = TaxaMinima;
        var superior = TaxaMaxima;
        var vplInferior = VplEm(inferior);
        var vplSuperior = VplEm(superior);

        if (Math.Sign(vplInferior) == Math.Sign(vplSuperior))
        {
            return null;
        }

        for (var iteracao = 0; iteracao < MaxIteracoesBisseccao; iteracao++)
        {
            var meio = (inferior + superior) / 2.0;
            var vplMeio = VplEm(meio);

            if (Math.Abs(vplMeio) < ToleranciaVpl)
            {
                return (decimal)meio;
            }

            if (Math.Sign(vplMeio) == Math.Sign(vplInferior))
            {
                inferior = meio;
                vplInferior = vplMeio;
            }
            else
            {
                superior = meio;
            }
        }

        return (decimal)((inferior + superior) / 2.0);
    }
}
