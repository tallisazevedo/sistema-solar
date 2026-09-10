using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SolarES.Aplicacao.Propostas;
using SolarES.Dominio.Configuracao;
using SolarES.Dominio.Simulacao;

namespace SolarES.Infraestrutura.Pdf;

/// <summary>
/// Renderiza a proposta em PDF a partir de dados ja congelados (ResultadoSimulacao e
/// ConfiguracaoCalculo da versao gravada na simulacao) -- nunca recalcula nada
/// (CLAUDE.md: "recalcular uma proposta antiga tem que reproduzir o numero original").
/// Licenca QuestPDF Community configurada uma vez em Program.cs.
/// </summary>
public sealed class GeradorPdfProposta : IGeradorPdfProposta
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public byte[] Gerar(string numero, DateTimeOffset validaAte, ConfiguracaoCalculo configuracao, ResultadoSimulacao resultado)
    {
        var possuiPremissaProvisoria = configuracao.PossuiPremissaProvisoria();

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(estilo => estilo.FontSize(10));

                if (possuiPremissaProvisoria)
                {
                    page.Foreground().Element(ComporMarcaDagua);
                }

                page.Header().Element(c => ComporCabecalho(c, numero));
                page.Content().Element(c => ComporConteudo(c, resultado));
                page.Footer().Element(c => ComporRodape(c, configuracao, validaAte));
            });
        });

        return documento.GeneratePdf();
    }

    private static void ComporMarcaDagua(IContainer container)
    {
        container
            .AlignCenter()
            .AlignMiddle()
            .Rotate(-30)
            .Text("CALIBRACAO PENDENTE — SUJEITO A VALIDACAO")
            .FontSize(28)
            .FontColor(Colors.Grey.Lighten2)
            .Bold();
    }

    private static void ComporCabecalho(IContainer container, string numero)
    {
        container.Column(coluna =>
        {
            coluna.Item().Text("Proposta SolarES").FontSize(18).Bold();
            coluna.Item().Text(string.Create(PtBr, $"Numero {numero}")).FontSize(11);
        });
    }

    private static void ComporConteudo(IContainer container, ResultadoSimulacao resultado)
    {
        container.PaddingVertical(10).Column(coluna =>
        {
            coluna.Spacing(12);

            coluna.Item().Element(c => ComporDimensionamento(c, resultado));
            coluna.Item().Element(c => ComporFinanceiro(c, resultado));
            coluna.Item().Element(c => ComporProjecao(c, resultado));
        });
    }

    private static void ComporDimensionamento(IContainer container, ResultadoSimulacao resultado)
    {
        container.Column(coluna =>
        {
            coluna.Item().Text("Dimensionamento").FontSize(13).Bold();
            coluna.Item().Text(string.Create(PtBr, $"Potencia instalada: {resultado.PotenciaInstaladaKwp:N2} kWp"));
            coluna.Item().Text(string.Create(PtBr, $"Quantidade de modulos: {resultado.QuantidadeModulos}"));
            coluna.Item().Text(string.Create(PtBr, $"Area necessaria: {resultado.AreaNecessariaM2:N2} m2"));
            coluna.Item().Text(string.Create(PtBr, $"Cobertura: {resultado.CoberturaPercentual:N0}%"));
            coluna.Item().Text(string.Create(PtBr, $"Investimento (capex): R$ {resultado.Capex:N2}"));

            if (resultado.RoteadaParaHumano)
            {
                coluna.Item().Text(string.Create(PtBr, $"Atencao: {resultado.MotivoRoteamento}")).FontColor(Colors.Orange.Darken2).Bold();
            }
        });
    }

    private static void ComporFinanceiro(IContainer container, ResultadoSimulacao resultado)
    {
        var paybackSimplesTexto = resultado.PaybackMesesSimples.HasValue
            ? string.Create(PtBr, $"{resultado.PaybackMesesSimples} meses")
            : "nao atingido no horizonte";
        var paybackDescontadoTexto = resultado.PaybackMesesDescontado.HasValue
            ? string.Create(PtBr, $"{resultado.PaybackMesesDescontado} meses")
            : "nao atingido no horizonte";
        var tirTexto = resultado.Tir.HasValue
            ? string.Create(PtBr, $"{resultado.Tir:P1}")
            : "sem raiz no intervalo analisado";

        container.Column(coluna =>
        {
            coluna.Item().Text("Retorno financeiro").FontSize(13).Bold();
            coluna.Item().Text(string.Create(PtBr, $"Economia media mensal (ano 1): R$ {resultado.EconomiaMensalAno1:N2}"));
            coluna.Item().Text(string.Create(PtBr, $"Payback simples: {paybackSimplesTexto}"));
            coluna.Item().Text(string.Create(PtBr, $"Payback descontado: {paybackDescontadoTexto}"));
            coluna.Item().Text(string.Create(PtBr, $"VPL: R$ {resultado.Vpl:N2}"));
            coluna.Item().Text(string.Create(PtBr, $"TIR: {tirTexto}"));
        });
    }

    private static void ComporProjecao(IContainer container, ResultadoSimulacao resultado)
    {
        container.Column(coluna =>
        {
            coluna.Item().Text("Projecao ao longo do horizonte").FontSize(13).Bold();
            coluna.Item().Text("A economia liquida anual diminui ao longo do tempo pela degradacao do modulo e pelo avanco do cronograma do Fio B.").FontSize(9);

            coluna.Item().Table(tabela =>
            {
                tabela.ColumnsDefinition(colunas =>
                {
                    colunas.RelativeColumn();
                    colunas.RelativeColumn();
                    colunas.RelativeColumn();
                });

                tabela.Header(cabecalho =>
                {
                    cabecalho.Cell().Text("Ano").Bold();
                    cabecalho.Cell().Text("Geracao (kWh)").Bold();
                    cabecalho.Cell().Text("Economia liquida (R$)").Bold();
                });

                foreach (var ano in resultado.Projecao)
                {
                    tabela.Cell().Text(ano.AnoCalendario.ToString(PtBr));
                    tabela.Cell().Text(ano.GeracaoAnualKwh.ToString("N0", PtBr));
                    tabela.Cell().Text(ano.EconomiaLiquidaAnualReais.ToString("N2", PtBr));
                }
            });
        });
    }

    private static void ComporRodape(IContainer container, ConfiguracaoCalculo configuracao, DateTimeOffset validaAte)
    {
        container.Column(coluna =>
        {
            coluna.Item().Text(string.Create(PtBr, $"Valida ate {validaAte:dd/MM/yyyy}.")).FontSize(9).Bold();
            coluna.Item().Text(configuracao.TextosProposta.Valor.Disclaimer).FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }
}
