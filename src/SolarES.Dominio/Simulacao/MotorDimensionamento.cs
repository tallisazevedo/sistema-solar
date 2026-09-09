using SolarES.Dominio.Catalogo;
using SolarES.Dominio.Configuracao;

namespace SolarES.Dominio.Simulacao;

/// <summary>
/// Dimensionamento do sistema fotovoltaico (docs/02, secao "Dimensionamento").
/// Funcao pura: nada aqui le banco, arquivo ou relogio -- tudo materializado pelo
/// chamador (docs/03, regra central do isolamento do Dominio).
/// </summary>
public static class MotorDimensionamento
{
    public static ResultadoDimensionamento Dimensionar(
        EntradaSimulacao entrada,
        ConfiguracaoCalculo configuracao,
        decimal hspMedioAnual,
        ModuloFotovoltaico modulo,
        IReadOnlyList<Inversor> catalogoInversores)
    {
        var custoDisponibilidade = configuracao.CustoDisponibilidadePorLigacao.Valor.ParaLigacao(entrada.TipoLigacao);
        var consumoCompensavel = Math.Max(0m, entrada.ConsumoMedioMensal - custoDisponibilidade);

        var consumoDiario = consumoCompensavel / 30m;
        var geracaoPorKwpDia = hspMedioAnual * configuracao.PerformanceRatio.Valor * configuracao.FatorOrientacaoPadrao.Valor;
        var potenciaNecessariaKwp = consumoDiario / geracaoPorKwpDia;

        var quantidadeModulos = (int)Math.Ceiling(potenciaNecessariaKwp * 1000m / modulo.PotenciaW);
        var potenciaInstaladaKwp = quantidadeModulos * modulo.PotenciaW / 1000m;

        var areaModuloM2 = (modulo.LarguraMm / 1000m) * (modulo.AlturaMm / 1000m);
        var areaNecessariaM2 = quantidadeModulos * areaModuloM2;

        var inversorEscolhidoId = quantidadeModulos > 0
            ? EscolherInversor(potenciaInstaladaKwp, catalogoInversores, configuracao.OversizingMaximo.Valor)
            : null;

        return new ResultadoDimensionamento(
            consumoCompensavel,
            potenciaNecessariaKwp,
            quantidadeModulos,
            potenciaInstaladaKwp,
            areaNecessariaM2,
            modulo.Id,
            inversorEscolhidoId);
    }

    private static Guid? EscolherInversor(decimal potenciaInstaladaKwp, IReadOnlyList<Inversor> catalogo, decimal oversizingMaximo)
    {
        return catalogo
            .Where(inversor => potenciaInstaladaKwp / (inversor.PotenciaW / 1000m) <= oversizingMaximo)
            .OrderBy(inversor => inversor.PotenciaW)
            .Select(inversor => (Guid?)inversor.Id)
            .FirstOrDefault();
    }
}
