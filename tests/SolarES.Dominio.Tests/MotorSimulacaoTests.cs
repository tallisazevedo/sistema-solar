using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Precificacao;
using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

public class MotorSimulacaoTests
{
    private static readonly ConfiguracaoCalculo Configuracao = ConfiguracaoCalculoBaseline.Criar();

    private const decimal HspMedioAnual = 5.0m;
    private const decimal TarifaCheia = 0.95m;
    private const decimal ValorFioBPorKwh = 0.21m;
    private const int AnoCalendarioInicial = 2026;

    // Vitoria -- na lista do kit litoral do baseline (T03/docs/02).
    private const string MunicipioListaKitLitoral = "3205309";
    private const string MunicipioForaDaLista = "9999999";

    private static ModuloFotovoltaico CriarModulo() => new()
    {
        Id = Guid.NewGuid(),
        Fabricante = "Fabricante Teste",
        Modelo = "Modelo Teste",
        PotenciaW = 550,
        LarguraMm = 1134,
        AlturaMm = 2278,
        EficienciaPercentual = 21.3m,
        ResistenteNevoaSalina = false,
        Ativo = true,
    };

    private static Inversor CriarInversor(int potenciaW) => new()
    {
        Id = Guid.NewGuid(),
        Fabricante = "Fabricante Teste",
        Modelo = $"Inversor {potenciaW}W",
        PotenciaW = potenciaW,
        QuantidadeMppt = 2,
        Tipo = TipoInversor.String,
        Ativo = true,
    };

    private static FaixaPreco CriarFaixaPreco(decimal kwpMinimo, decimal kwpMaximo, decimal precoPorWp, bool kitLitoral) => new()
    {
        Id = Guid.NewGuid(),
        KwpMinimo = kwpMinimo,
        KwpMaximo = kwpMaximo,
        PrecoPorWp = precoPorWp,
        TipoInstalacao = "Residencial",
        KitLitoral = kitLitoral,
        Vigencia = DateTimeOffset.UtcNow,
    };

    private static EntradaSimulacao CriarEntrada(
        decimal consumoMedioMensal = 500m,
        TipoLigacao ligacao = TipoLigacao.Monofasica,
        string municipioCodigoIbge = MunicipioForaDaLista,
        decimal areaDisponivelM2 = 1000m,
        bool possuiGeracaoPropria = false) => new(
            Enumerable.Repeat(consumoMedioMensal, 12).ToList(),
            ligacao,
            Subgrupo.B1,
            municipioCodigoIbge,
            TipoTelhado.Ceramico,
            areaDisponivelM2,
            possuiGeracaoPropria);

    private static ResultadoSimulacao Simular(
        EntradaSimulacao? entrada = null,
        decimal distanciaMarKmMunicipio = 200m,
        IReadOnlyList<FaixaPreco>? catalogoFaixasPreco = null) =>
        MotorSimulacao.Simular(
            entrada ?? CriarEntrada(),
            Configuracao,
            HspMedioAnual,
            Enumerable.Repeat(HspMedioAnual, 12).ToList(),
            distanciaMarKmMunicipio,
            CriarModulo(),
            [CriarInversor(10000)],
            catalogoFaixasPreco ?? [CriarFaixaPreco(0m, 100m, 3.5m, kitLitoral: false), CriarFaixaPreco(0m, 100m, 4.2m, kitLitoral: true)],
            TarifaCheia,
            ValorFioBPorKwh,
            AnoCalendarioInicial);

    [Fact]
    public void Simular_CaminhoFeliz_ProduzResultadoCompletoSemRoteamento()
    {
        var resultado = Simular();

        Assert.True(resultado.PotenciaInstaladaKwp > 0);
        Assert.True(resultado.QuantidadeModulos > 0);
        Assert.Equal(100m, resultado.CoberturaPercentual);
        Assert.True(resultado.Capex > 0);
        Assert.True(resultado.EconomiaMensalAno1 > 0);
        Assert.False(resultado.RoteadaParaHumano);
        Assert.Null(resultado.MotivoRoteamento);
    }

    [Fact]
    public void Simular_ComGeracaoPropria_RoteiaParaHumano()
    {
        var entrada = CriarEntrada(possuiGeracaoPropria: true);

        var resultado = Simular(entrada);

        Assert.True(resultado.RoteadaParaHumano);
        Assert.Contains("geracao propria", resultado.MotivoRoteamento, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Simular_ComPotenciaAcimaDoLimiteConfigurado_RoteiaParaHumano()
    {
        // Consumo alto o suficiente pra dimensionar acima do LimiteKwpRoteamentoHumano do baseline (75 kWp).
        var entrada = CriarEntrada(consumoMedioMensal: 20000m, areaDisponivelM2: 100_000m);
        var faixasAmplas = new[] { CriarFaixaPreco(0m, 500m, 3.5m, kitLitoral: false) };

        var resultado = Simular(entrada, catalogoFaixasPreco: faixasAmplas);

        Assert.True(resultado.PotenciaInstaladaKwp > Configuracao.LimiteKwpRoteamentoHumano.Valor);
        Assert.True(resultado.RoteadaParaHumano);
        Assert.Contains("kwp", resultado.MotivoRoteamento, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Simular_SemFaixaDePrecoCobrindoOKwpDimensionado_RoteiaParaHumanoComCapexZero()
    {
        var resultado = Simular(catalogoFaixasPreco: []);

        Assert.Equal(0m, resultado.Capex);
        Assert.True(resultado.RoteadaParaHumano);
        Assert.Contains("faixa de preco", resultado.MotivoRoteamento, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Simular_ComAreaInsuficiente_AplicaCoberturaParcial()
    {
        var entrada = CriarEntrada(consumoMedioMensal: 1500m, areaDisponivelM2: 5m);

        var resultado = Simular(entrada);

        Assert.True(resultado.AreaNecessariaM2 > 5m);
        Assert.True(resultado.CoberturaPercentual < 100m);
        Assert.True(resultado.QuantidadeModulos > 0);
    }

    [Fact]
    public void Simular_MunicipioNaListaDoKitLitoral_UsaFaixaDePrecoDeKitLitoral()
    {
        var entradaLitoral = CriarEntrada(municipioCodigoIbge: MunicipioListaKitLitoral);
        var entradaNaoLitoral = CriarEntrada(municipioCodigoIbge: MunicipioForaDaLista);
        // distanciaMarKm grande o suficiente pra nao disparar kit litoral por raio -- so a lista deve valer aqui.
        var distanciaForaDoRaio = Configuracao.KitLitoral.Valor.RaioKm + 100m;

        var resultadoLitoral = Simular(entradaLitoral, distanciaMarKmMunicipio: distanciaForaDoRaio);
        var resultadoNaoLitoral = Simular(entradaNaoLitoral, distanciaMarKmMunicipio: distanciaForaDoRaio);

        var precoPorWpLitoral = resultadoLitoral.Capex / (resultadoLitoral.PotenciaInstaladaKwp * 1000m);
        var precoPorWpNaoLitoral = resultadoNaoLitoral.Capex / (resultadoNaoLitoral.PotenciaInstaladaKwp * 1000m);
        Assert.True(precoPorWpLitoral > precoPorWpNaoLitoral);
    }

    [Fact]
    public void Simular_MunicipioDentroDoRaioDoKitLitoral_UsaFaixaDePrecoDeKitLitoral()
    {
        var entrada = CriarEntrada(municipioCodigoIbge: MunicipioForaDaLista);
        var distanciaDentroDoRaio = Configuracao.KitLitoral.Valor.RaioKm - 1m;

        var resultado = Simular(entrada, distanciaMarKmMunicipio: distanciaDentroDoRaio);

        var precoPorWp = resultado.Capex / (resultado.PotenciaInstaladaKwp * 1000m);
        Assert.Equal(4.2m, precoPorWp);
    }
}
