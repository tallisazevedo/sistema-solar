using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Precificacao;

namespace SolarES.Dominio.Simulacao;

/// <summary>
/// Orquestra os motores individuais (T08-T14) numa unica simulacao, na ordem:
/// dimensionamento -> cobertura parcial (area) -> roteamento humano -> kit litoral ->
/// geracao mensal (sobre a potencia ja ajustada pela cobertura parcial) -> Fio B do
/// ano 1 -> projecao do horizonte -> precificacao -> financeiro. Funcao pura, mesmo
/// padrao dos demais motores -- nada aqui le banco, arquivo ou relogio.
/// </summary>
public static class MotorSimulacao
{
    public static ResultadoSimulacao Simular(
        EntradaSimulacao entrada,
        ConfiguracaoCalculo configuracao,
        decimal hspMedioAnual,
        IReadOnlyList<decimal> hspPorMes,
        decimal distanciaMarKmMunicipio,
        ModuloFotovoltaico modulo,
        IReadOnlyList<Inversor> catalogoInversoresAtivos,
        IReadOnlyList<FaixaPreco> catalogoFaixasPreco,
        decimal tarifaCheia,
        decimal valorFioBPorKwh,
        int anoCalendarioInicial)
    {
        var dimensionamento = MotorDimensionamento.Dimensionar(
            entrada, configuracao, hspMedioAnual, modulo, catalogoInversoresAtivos);

        var cobertura = MotorCoberturaParcial.Ajustar(dimensionamento, modulo, entrada.AreaDisponivelM2);

        var (roteada, motivoRoteamento) = MotorRoteamentoHumano.Avaliar(
            entrada, configuracao, dimensionamento.PotenciaInstaladaKwp);

        var kitLitoral = MotorKitLitoral.Aplica(configuracao, entrada.MunicipioCodigoIbge, distanciaMarKmMunicipio);

        var geracaoMensal = MotorGeracaoMensal.Calcular(
            entrada, configuracao, hspPorMes, cobertura.PotenciaInstaladaFinalKwp);

        var economiaMensalAno1 = MotorFioB.Calcular(
            geracaoMensal, configuracao, anoCalendarioInicial, valorFioBPorKwh, tarifaCheia);
        var economiaMensalAno1Media = economiaMensalAno1.Count == 0
            ? 0m
            : economiaMensalAno1.Sum(e => e.EconomiaLiquidaReais) / economiaMensalAno1.Count;

        var projecao = MotorProjecao.Projetar(
            geracaoMensal, configuracao, anoCalendarioInicial, tarifaCheia, valorFioBPorKwh);

        var capex = MotorPrecificacao.CalcularCapex(cobertura.PotenciaInstaladaFinalKwp, kitLitoral, catalogoFaixasPreco);

        // Sem faixa de preco cadastrada pro kWp dimensionado, nao ha proposta possivel --
        // vira mais um motivo de roteamento humano em vez de um capex inventado (mesmo
        // espirito do MotorFinanceiro: sem raiz, TIR volta null, nunca um numero sem sentido).
        if (capex is null && !roteada)
        {
            roteada = true;
            motivoRoteamento = $"Nenhuma faixa de preco configurada para {cobertura.PotenciaInstaladaFinalKwp:F2} kWp.";
        }

        var financeiro = capex is null
            ? null
            : MotorFinanceiro.Calcular(
                projecao.Select(a => a.EconomiaLiquidaAnualReais).ToList(), capex.Value, configuracao.TaxaDesconto.Valor);

        return new ResultadoSimulacao(
            cobertura.PotenciaInstaladaFinalKwp,
            cobertura.QuantidadeModulosFinal,
            dimensionamento.AreaNecessariaM2,
            cobertura.CoberturaPercentual,
            capex ?? 0m,
            economiaMensalAno1Media,
            financeiro?.PaybackMesesSimples,
            financeiro?.PaybackMesesDescontado,
            financeiro?.Tir,
            financeiro?.Vpl ?? 0m,
            roteada,
            motivoRoteamento);
    }
}
