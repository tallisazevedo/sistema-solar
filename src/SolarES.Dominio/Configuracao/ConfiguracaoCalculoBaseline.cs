using SolarES.Dominio.Premissas;

namespace SolarES.Dominio.Configuracao;

/// <summary>
/// Primeira ConfiguracaoCalculo do projeto, antes de qualquer validacao com a
/// empresa. Cada valor Provisorio carrega a justificativa do chute; a promocao para
/// valor validado so acontece no portao G1 (docs/05).
/// </summary>
public static class ConfiguracaoCalculoBaseline
{
    public static ConfiguracaoCalculo Criar() => new(
        PerformanceRatio: new Premissa<decimal>(
            0.78m,
            OrigemPremissa.Provisorio,
            "Meio da faixa tipica (0,75-0,80) citada no docs/02; a validar no G1."),

        DegradacaoAnual: new Premissa<decimal>(
            0.005m,
            OrigemPremissa.Provisorio,
            "Ordem de grandeza tipica de degradacao de modulo citada no docs/02."),

        InflacaoTarifaria: new Premissa<decimal>(
            0.08m,
            OrigemPremissa.Provisorio,
            "Premissa comercial sem numero definido no docs/02; chute baseado no historico de reajuste B1 no Brasil. Decisao do dono no G1."),

        TaxaDesconto: new Premissa<decimal>(
            0.12m,
            OrigemPremissa.Provisorio,
            "Premissa comercial; proxy de custo de capital para pequena integradora. Decisao do dono no G1."),

        HorizonteAnos: new Premissa<int>(
            25,
            OrigemPremissa.Provisorio,
            "Janela padrao do setor de geracao distribuida (garantia tipica de modulo); ajustavel no admin."),

        OversizingMaximo: new Premissa<decimal>(
            1.45m,
            OrigemPremissa.Provisorio,
            "Pratica comum do mercado brasileiro de integradoras; a validar com o dono."),

        FatorOrientacaoPadrao: new Premissa<decimal>(
            0.95m,
            OrigemPremissa.Provisorio,
            "Fator unico assumido para orientacao/inclinacao 'boa o suficiente', sem a tabela azimute x inclinacao ainda (docs/02 ja anota isso como proximo refino)."),

        CronogramaFioB: new Premissa<IReadOnlyList<PercentualFioBAno>>(
            new List<PercentualFioBAno>
            {
                new(2023, 0.15m),
                new(2024, 0.30m),
                new(2025, 0.45m),
                new(2026, 0.60m),
                new(2027, 0.75m),
                new(2028, 0.90m),
            },
            OrigemPremissa.Lei,
            "Art. 27 da Lei 14.300/2022."),

        EstrategiaFioBForaCronograma: new Premissa<EstrategiaFioBForaCronograma>(
            Configuracao.EstrategiaFioBForaCronograma.MantemUltimoPercentual,
            OrigemPremissa.Provisorio,
            "ANEEL ainda nao definiu a metodologia do art. 17 para 2029+; mantem o ultimo percentual conhecido (90%) como fallback conservador. Nunca assume 100%."),

        LimiteKwpRoteamentoHumano: new Premissa<decimal>(
            75m,
            OrigemPremissa.Provisorio,
            "Limite regulatorio de microgeracao (REN ANEEL) como ponto de partida; criterio final e do dono."),

        KitLitoral: new Premissa<ConfiguracaoKitLitoral>(
            new ConfiguracaoKitLitoral(
                new List<string>
                {
                    "3205309", // Vitoria
                    "3205200", // Vila Velha
                    "3205002", // Serra
                    "3201308", // Cariacica
                    "3202405", // Guarapari
                    "3200409", // Anchieta
                    "3204203", // Piuma
                    "3203320", // Marataizes
                    "3204302", // Presidente Kennedy
                    "3200607", // Aracruz
                    "3202207", // Fundao
                    "3203205", // Linhares
                    "3204906", // Sao Mateus
                    "3201605", // Conceicao da Barra
                },
                RaioKm: 5m),
            OrigemPremissa.Provisorio,
            "Lista literal de municipios candidatos do docs/02 (criterio do dono a validar); raio de 5 km como chute inicial."),

        CustoDisponibilidadePorLigacao: new Premissa<CustoDisponibilidade>(
            new CustoDisponibilidade(Monofasica: 30m, Bifasica: 50m, Trifasica: 100m),
            OrigemPremissa.Lei,
            "Art. 5 da Lei 14.300/2022 / REN ANEEL 1.059/2023 — valores citados literalmente no docs/02."));
}
