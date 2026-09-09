using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

public class MotorDimensionamentoTests
{
    private static readonly ConfiguracaoCalculo Configuracao = ConfiguracaoCalculoBaseline.Criar();

    private const decimal HspMedioAnual = 5.0m;

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

    private static EntradaSimulacao CriarEntrada(decimal consumoMedioMensal, TipoLigacao ligacao) => new(
        Enumerable.Repeat(consumoMedioMensal, 12).ToList(),
        ligacao,
        Subgrupo.B1,
        "3205309",
        TipoTelhado.Ceramico,
        100m,
        false);

    [Theory]
    [InlineData(TipoLigacao.Monofasica, 30)]
    [InlineData(TipoLigacao.Bifasica, 50)]
    [InlineData(TipoLigacao.Trifasica, 100)]
    public void Dimensionar_SubtraiCustoDisponibilidadeDaLigacao(TipoLigacao ligacao, decimal custoDisponibilidade)
    {
        var entrada = CriarEntrada(500m, ligacao);

        var resultado = MotorDimensionamento.Dimensionar(
            entrada, Configuracao, HspMedioAnual, CriarModulo(), [CriarInversor(5000)]);

        Assert.Equal(500m - custoDisponibilidade, resultado.ConsumoCompensavel);
    }

    [Fact]
    public void Dimensionar_ConsumoAbaixoDoCustoDisponibilidade_RetornaZeroCompensavelNuncaNegativo()
    {
        var entrada = CriarEntrada(20m, TipoLigacao.Monofasica);

        var resultado = MotorDimensionamento.Dimensionar(
            entrada, Configuracao, HspMedioAnual, CriarModulo(), [CriarInversor(5000)]);

        Assert.Equal(0m, resultado.ConsumoCompensavel);
        Assert.Equal(0, resultado.QuantidadeModulos);
        Assert.Equal(0m, resultado.PotenciaInstaladaKwp);
    }

    [Fact]
    public void Dimensionar_ComConsumoAlto_EscolheInversorDentroDoOversizingMaximo()
    {
        var entrada = CriarEntrada(1500m, TipoLigacao.Trifasica);
        var catalogoInversores = new[] { CriarInversor(3000), CriarInversor(6000), CriarInversor(10000) };

        var resultado = MotorDimensionamento.Dimensionar(
            entrada, Configuracao, HspMedioAnual, CriarModulo(), catalogoInversores);

        Assert.True(resultado.QuantidadeModulos > 0);
        Assert.NotNull(resultado.InversorEscolhidoId);

        var inversorEscolhido = catalogoInversores.Single(i => i.Id == resultado.InversorEscolhidoId);
        var oversizing = resultado.PotenciaInstaladaKwp / (inversorEscolhido.PotenciaW / 1000m);
        Assert.True(oversizing <= Configuracao.OversizingMaximo.Valor);
    }

    [Fact]
    public void Dimensionar_QuandoNenhumInversorAtendeOOversizing_RetornaNuloSemLancar()
    {
        var entrada = CriarEntrada(1500m, TipoLigacao.Trifasica);
        var catalogoInversores = new[] { CriarInversor(500) };

        var resultado = MotorDimensionamento.Dimensionar(
            entrada, Configuracao, HspMedioAnual, CriarModulo(), catalogoInversores);

        Assert.Null(resultado.InversorEscolhidoId);
    }

    [Fact]
    public void Dimensionar_AreaNecessaria_CalculaDaAreaDoModulo()
    {
        var entrada = CriarEntrada(1500m, TipoLigacao.Trifasica);
        var modulo = CriarModulo();

        var resultado = MotorDimensionamento.Dimensionar(
            entrada, Configuracao, HspMedioAnual, modulo, [CriarInversor(10000)]);

        var areaModuloM2 = (modulo.LarguraMm / 1000m) * (modulo.AlturaMm / 1000m);
        Assert.Equal(resultado.QuantidadeModulos * areaModuloM2, resultado.AreaNecessariaM2);
    }
}
