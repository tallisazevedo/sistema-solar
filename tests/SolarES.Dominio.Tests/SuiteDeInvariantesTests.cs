using System.Diagnostics;
using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

/// <summary>
/// Suite de invariantes (docs/05, T14): substitui a suite de regressao enquanto nao
/// ha gabarito real (G1). Gera entradas aleatorias dentro de faixas realistas
/// (seed fixa — reprodutivel, nao "flaky") e verifica propriedades que precisam
/// valer para qualquer combinacao de parametros, nao so para os exemplos fixos das
/// T08-T13.
/// </summary>
public class SuiteDeInvariantesTests
{
    private const int NumeroDeCasos = 500;
    private static readonly ConfiguracaoCalculo Configuracao = ConfiguracaoCalculoBaseline.Criar();

    private sealed record Cenario(
        EntradaSimulacao Entrada,
        IReadOnlyList<decimal> HspPorMes,
        decimal HspMedioAnual,
        ModuloFotovoltaico Modulo,
        IReadOnlyList<Inversor> Inversores,
        decimal TarifaCheia,
        decimal ValorFioBPorKwh,
        int AnoCalendarioInicial,
        decimal PrecoPorWp);

    private static Cenario GerarCenarioAleatorio(Random rng, decimal? consumoMedioForcado = null)
    {
        var tipoLigacao = (TipoLigacao)rng.Next(0, 3);

        var consumoBase = consumoMedioForcado ?? (decimal)(rng.NextDouble() * 1900 + 100); // 100-2000 kWh
        var historico = Enumerable.Range(0, 12)
            .Select(_ => Math.Max(0m, consumoBase * (decimal)(0.8 + rng.NextDouble() * 0.4)))
            .ToList();

        var entrada = new EntradaSimulacao(
            historico,
            tipoLigacao,
            Subgrupo.B1,
            "3205309",
            TipoTelhado.Ceramico,
            areaDisponivelM2: 1000m, // area generosa: T14 nao testa cobertura parcial (isso e T13)
            possuiGeracaoPropria: false);

        var hspBase = (decimal)(rng.NextDouble() * 2.5 + 4.0); // 4,0-6,5 kWh/m2/dia (faixa real do ES, T06)
        var hspPorMes = Enumerable.Range(0, 12)
            .Select(_ => Math.Max(0.1m, hspBase * (decimal)(0.7 + rng.NextDouble() * 0.6)))
            .ToList();

        var modulo = new ModuloFotovoltaico
        {
            Id = Guid.NewGuid(),
            Fabricante = "Fabricante Aleatorio",
            Modelo = "Modelo Aleatorio",
            PotenciaW = rng.Next(350, 601),
            LarguraMm = 1100,
            AlturaMm = 2200,
            EficienciaPercentual = 21m,
            ResistenteNevoaSalina = false,
            Ativo = true,
        };

        var inversores = Enumerable.Range(0, 3)
            .Select(i => new Inversor
            {
                Id = Guid.NewGuid(),
                Fabricante = "Fabricante Aleatorio",
                Modelo = $"Inversor {i}",
                PotenciaW = 3000 + i * 4000, // 3, 7, 11 kWp -- cobre oversizing variado
                QuantidadeMppt = 2,
                Tipo = TipoInversor.String,
                Ativo = true,
            })
            .ToList();

        // valorFioB sempre estritamente menor que tarifaCheia, por construcao --
        // e a relacao real de qualquer distribuidora (Fio B e subcomponente da TUSD).
        var valorFioBPorKwh = (decimal)(rng.NextDouble() * 0.20 + 0.10); // 0,10-0,30
        var tarifaCheia = valorFioBPorKwh + (decimal)(rng.NextDouble() * 0.40 + 0.30); // soma positiva

        var anoCalendarioInicial = rng.Next(2024, 2035);
        var precoPorWp = (decimal)(rng.NextDouble() * 3.0 + 3.0); // R$ 3,00-6,00/Wp

        return new Cenario(entrada, hspPorMes, hspPorMes.Average(), modulo, inversores, tarifaCheia, valorFioBPorKwh, anoCalendarioInicial, precoPorWp);
    }

    private static (ResultadoDimensionamento Dimensionamento, IReadOnlyList<GeracaoMes> GeracaoMensal, IReadOnlyList<AnoProjecao> Projecao, ResultadoFinanceiro Financeiro) RodarPipeline(Cenario cenario)
    {
        var dimensionamento = MotorDimensionamento.Dimensionar(cenario.Entrada, Configuracao, cenario.HspMedioAnual, cenario.Modulo, cenario.Inversores);
        var geracaoMensal = MotorGeracaoMensal.Calcular(cenario.Entrada, Configuracao, cenario.HspPorMes, dimensionamento.PotenciaInstaladaKwp);
        var projecao = MotorProjecao.Projetar(geracaoMensal, Configuracao, cenario.AnoCalendarioInicial, cenario.TarifaCheia, cenario.ValorFioBPorKwh);

        var capex = dimensionamento.PotenciaInstaladaKwp * 1000m * cenario.PrecoPorWp;
        var fluxosAnuais = projecao.Select(ano => ano.EconomiaLiquidaAnualReais).ToList();
        var financeiro = MotorFinanceiro.Calcular(fluxosAnuais, capex, Configuracao.TaxaDesconto.Valor);

        return (dimensionamento, geracaoMensal, projecao, financeiro);
    }

    [Fact]
    public void Invariante1_EconomiaNuncaAtingeCemPorCentoDaConta()
    {
        var rng = new Random(1001);
        var casosVerificados = 0;
        var cronometro = Stopwatch.StartNew();

        for (var i = 0; i < NumeroDeCasos; i++)
        {
            var cenario = GerarCenarioAleatorio(rng);
            var (_, geracaoMensal, _, _) = RodarPipeline(cenario);

            foreach (var mes in geracaoMensal)
            {
                var consumoDoMes = cenario.Entrada.HistoricoConsumoKwh[mes.Mes - 1];
                if (consumoDoMes > 0)
                {
                    Assert.True(mes.EnergiaCompensadaKwh < consumoDoMes,
                        $"Energia compensada ({mes.EnergiaCompensadaKwh}) atingiu ou superou 100% do consumo ({consumoDoMes}) no mes {mes.Mes}.");
                }
            }

            casosVerificados++;
        }

        cronometro.Stop();
        Assert.Equal(NumeroDeCasos, casosVerificados);
    }

    [Fact]
    public void Invariante2_PotenciaDimensionadaCresceMonotonicamenteComOConsumo()
    {
        var rng = new Random(1002);

        for (var i = 0; i < NumeroDeCasos; i++)
        {
            var consumoMenor = (decimal)(rng.NextDouble() * 900 + 100); // 100-1000
            var consumoMaior = consumoMenor + (decimal)(rng.NextDouble() * 900 + 50); // estritamente maior

            var seedComum = rng.Next();
            var cenarioMenor = GerarCenarioAleatorio(new Random(seedComum), consumoMenor);
            var cenarioMaior = GerarCenarioAleatorio(new Random(seedComum), consumoMaior);

            var (dimensionamentoMenor, _, _, _) = RodarPipeline(cenarioMenor);
            var (dimensionamentoMaior, _, _, _) = RodarPipeline(cenarioMaior);

            Assert.True(dimensionamentoMaior.PotenciaNecessariaKwp >= dimensionamentoMenor.PotenciaNecessariaKwp,
                $"Consumo maior ({consumoMaior}) produziu potencia menor ({dimensionamentoMaior.PotenciaNecessariaKwp}) que consumo menor ({consumoMenor} -> {dimensionamentoMenor.PotenciaNecessariaKwp}).");
        }
    }

    [Fact]
    public void Invariante3_EconomiaAnualEmTermosReais_NuncaCresceAoLongoDoHorizonte()
    {
        var rng = new Random(1003);

        for (var i = 0; i < NumeroDeCasos; i++)
        {
            var cenario = GerarCenarioAleatorio(rng);
            var (_, _, projecao, _) = RodarPipeline(cenario);

            // Tolerancia de 1 centavo: quando geracao nao e o fator limitante em dois
            // anos consecutivos (consumo compensavel e o teto nos dois), a economia
            // real "deveria" ser identica -- mas cada ano divide por um fatorInflacao
            // ligeiramente diferente, e o arredondamento de decimal produz diferencas
            // na ordem de 1e-27, financeiramente nulas. Nao e a invariante quebrando.
            const decimal toleranciaReais = 0.01m;
            for (var ano = 1; ano < projecao.Count; ano++)
            {
                Assert.True(
                    projecao[ano].EconomiaLiquidaAnualEmTermosReaisReais <= projecao[ano - 1].EconomiaLiquidaAnualEmTermosReaisReais + toleranciaReais,
                    $"Economia real subiu do ano {projecao[ano - 1].Ano} ({projecao[ano - 1].EconomiaLiquidaAnualEmTermosReaisReais}) para o ano {projecao[ano].Ano} ({projecao[ano].EconomiaLiquidaAnualEmTermosReaisReais}).");
            }
        }
    }

    [Fact]
    public void Invariante4_CustoDoFioBNuncaSuperaAEconomiaBruta()
    {
        var rng = new Random(1004);

        for (var i = 0; i < NumeroDeCasos; i++)
        {
            var cenario = GerarCenarioAleatorio(rng);
            var (_, geracaoMensal, _, _) = RodarPipeline(cenario);

            var percentualAno1 = MotorFioB.ResolverPercentualAno(Configuracao, cenario.AnoCalendarioInicial);
            var economiaAno1 = MotorFioB.Calcular(geracaoMensal, Configuracao, cenario.AnoCalendarioInicial, cenario.ValorFioBPorKwh, cenario.TarifaCheia);

            foreach (var mes in economiaAno1)
            {
                Assert.True(mes.CustoFioBReais <= mes.EconomiaBrutaReais,
                    $"Custo do Fio B ({mes.CustoFioBReais}) superou a economia bruta ({mes.EconomiaBrutaReais}) no mes {mes.Mes} (percentual {percentualAno1}).");
            }
        }
    }

    [Fact]
    public void Invariante5_NenhumResultadoFinanceiroForaDosLimitesEsperados()
    {
        var rng = new Random(1005);

        for (var i = 0; i < NumeroDeCasos; i++)
        {
            var cenario = GerarCenarioAleatorio(rng);
            var (_, _, _, financeiro) = RodarPipeline(cenario);

            if (financeiro.PaybackMesesSimples is { } paybackSimples)
            {
                Assert.True(paybackSimples > 0, $"PaybackMesesSimples nao positivo: {paybackSimples}.");
            }

            if (financeiro.PaybackMesesDescontado is { } paybackDescontado)
            {
                Assert.True(paybackDescontado > 0, $"PaybackMesesDescontado nao positivo: {paybackDescontado}.");
            }

            if (financeiro.Tir is { } tir)
            {
                Assert.True(tir is >= -0.99m and <= 10.0m, $"TIR fora do intervalo de busca da bisseccao: {tir}.");
            }
        }
    }

    [Theory]
    [InlineData(TipoLigacao.Monofasica, 30)]
    [InlineData(TipoLigacao.Bifasica, 50)]
    [InlineData(TipoLigacao.Trifasica, 100)]
    public void Invariante6_ConsumoAbaixoDoCustoDisponibilidade_ProduzRecomendacaoDeNaoInstalar(TipoLigacao ligacao, decimal custoDisponibilidade)
    {
        var rng = new Random(1006);

        for (var i = 0; i < NumeroDeCasos; i++)
        {
            // Divide por 1,25 para garantir que MESMO o mes com a variacao sazonal
            // maxima (+20%) fique abaixo do custo de disponibilidade -- sem essa
            // margem, a media dos 12 meses ficaria abaixo, mas um mes isolado
            // poderia ultrapassar por sorte, tornando o teste instavel.
            var consumoAbaixoDoCusto = (decimal)rng.NextDouble() * (custoDisponibilidade / 1.25m);
            var cenario = GerarCenarioAleatorio(rng, consumoAbaixoDoCusto);
            var entradaComLigacaoForcada = new EntradaSimulacao(
                cenario.Entrada.HistoricoConsumoKwh,
                ligacao,
                cenario.Entrada.Subgrupo,
                cenario.Entrada.MunicipioCodigoIbge,
                cenario.Entrada.TipoTelhado,
                cenario.Entrada.AreaDisponivelM2,
                cenario.Entrada.PossuiGeracaoPropria);

            var dimensionamento = MotorDimensionamento.Dimensionar(entradaComLigacaoForcada, Configuracao, cenario.HspMedioAnual, cenario.Modulo, cenario.Inversores);

            Assert.Equal(0, dimensionamento.QuantidadeModulos);
            Assert.Equal(0m, dimensionamento.ConsumoCompensavel);
        }
    }
}
