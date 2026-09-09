using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Simulacao;

namespace SolarES.Dominio.Tests;

public class MotorCoberturaParcialTests
{
    private static ModuloFotovoltaico CriarModulo() => new()
    {
        Id = Guid.NewGuid(),
        Fabricante = "Fabricante Teste",
        Modelo = "Modelo Teste",
        PotenciaW = 550,
        LarguraMm = 1000,
        AlturaMm = 2000, // area = 2 m2 por modulo, numero redondo para facilitar o teste
        EficienciaPercentual = 21m,
        ResistenteNevoaSalina = false,
        Ativo = true,
    };

    private static ResultadoDimensionamento CriarDimensionamento(int quantidadeModulos) => new(
        ConsumoCompensavel: 1000m,
        PotenciaNecessariaKwp: quantidadeModulos * 0.55m,
        QuantidadeModulos: quantidadeModulos,
        PotenciaInstaladaKwp: quantidadeModulos * 0.55m,
        AreaNecessariaM2: quantidadeModulos * 2m,
        ModuloId: Guid.NewGuid(),
        InversorEscolhidoId: Guid.NewGuid());

    [Fact]
    public void Ajustar_ComAreaSuficiente_CoberturaEhCemPorCento()
    {
        var dimensionamento = CriarDimensionamento(quantidadeModulos: 10); // precisa de 20 m2
        var modulo = CriarModulo();

        var resultado = MotorCoberturaParcial.Ajustar(dimensionamento, modulo, areaDisponivelM2: 30m);

        Assert.Equal(100m, resultado.CoberturaPercentual);
        Assert.Equal(10, resultado.QuantidadeModulosFinal);
    }

    [Fact]
    public void Ajustar_ComAreaInsuficiente_DevolveCoberturaAbaixoDeCemSemReduzirEmSilencio()
    {
        var dimensionamento = CriarDimensionamento(quantidadeModulos: 10); // precisa de 20 m2
        var modulo = CriarModulo();

        var resultado = MotorCoberturaParcial.Ajustar(dimensionamento, modulo, areaDisponivelM2: 12m); // cabem so 6

        Assert.Equal(6, resultado.QuantidadeModulosFinal);
        Assert.True(resultado.QuantidadeModulosFinal < dimensionamento.QuantidadeModulos);
        Assert.Equal(60m, resultado.CoberturaPercentual);
        Assert.True(resultado.CoberturaPercentual < 100m);
    }
}
