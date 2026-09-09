# 02 — Domínio, glossário e regras

Este é o documento mais importante do repositório. Metade do vocabulário aqui é
regulação brasileira específica, sem tradução estável e sem intuição óbvia. Leia antes
de qualquer tarefa que toque no cálculo.

## Glossário

**kWp** — potência de pico do sistema fotovoltaico. Dimensiona o sistema. Não confundir
com kWh.

**kWh** — energia. É o que a conta de luz cobra e o que o sistema gera.

**HSP** — horas de sol pleno, em kWh/m²/dia. Quantas horas de irradiação a 1000 W/m²
equivalem à irradiação real do dia naquele local. Varia por município e por mês.

**PR (performance ratio)** — fator que desconta as perdas reais do sistema: temperatura,
sujeira, mismatch, cabeamento, eficiência do inversor. Tipicamente entre 0,75 e 0,80.

**UC** — unidade consumidora. Identificador do ponto de entrega na distribuidora.

**Grupo A / Grupo B** — Grupo A é média e alta tensão, com demanda contratada e postos
tarifários de ponta e fora-ponta. Grupo B é baixa tensão. **O MVP atende só o Grupo B.**

**Subgrupos B** — B1 residencial, B2 rural, B3 demais classes (comercial, industrial de
pequeno porte). O subgrupo muda tarifa e descontos.

**TE** — Tarifa de Energia. Componente que remunera a energia em si.

**TUSD** — Tarifa de Uso do Sistema de Distribuição. Remunera a rede. Se decompõe em
vários componentes, entre eles Fio A e Fio B.

**Fio B** — parcela da TUSD que remunera os ativos de distribuição. É sobre este
componente, e só sobre ele, que incide a cobrança da Lei 14.300. Seu valor em R$/kWh é
regional e muda a cada reajuste tarifário da distribuidora.

**Custo de disponibilidade** — consumo mínimo faturado pela distribuidora mesmo que o
sistema gere tudo. **Não é compensável.** Depende do tipo de ligação:

| Ligação | Custo de disponibilidade |
|---|---|
| Monofásica | 30 kWh |
| Bifásica | 50 kWh |
| Trifásica | 100 kWh |

Errar isso é o erro nº 1 em simuladores amadores: eles prometem economia de 100%, que
é impossível.

**Compensação / net metering** — o excedente injetado na rede vira crédito, abatido em
outros horários ou meses dentro da mesma área de concessão.

**Oversizing** — relação entre potência dos módulos e potência do inversor. Instalar
mais módulos do que o inversor comporta nominalmente é prática normal e desejável, até
um limite que é parâmetro configurável.

**Degradação** — perda anual de eficiência dos módulos, na ordem de 0,5% ao ano.
Entra na projeção de 25 anos.

**Parecer de acesso** — documento em que a distribuidora aprova a conexão do sistema.
Etapa mais lenta e mais opaca do processo.

**ART** — Anotação de Responsabilidade Técnica, emitida pelo engenheiro responsável.

**Névoa salina** — corrosão acelerada em instalações próximas ao mar. Exige módulos e
estruturas com resistência específica. Relevante em boa parte do litoral capixaba.

**Payback** — tempo até o investimento se pagar. Simples ignora o valor do dinheiro no
tempo; descontado considera.

**TIR / VPL** — taxa interna de retorno e valor presente líquido do fluxo de 25 anos.

## Lei 14.300 e o Fio B

A Lei 14.300/2022 é o marco legal da micro e minigeração distribuída. Ela criou uma
cobrança progressiva sobre a energia compensada, incidente **apenas sobre o componente
Fio B**, nunca sobre a tarifa cheia.

Cronograma do art. 27:

| Ano | Percentual do Fio B |
|---|---|
| 2023 | 15% |
| 2024 | 30% |
| 2025 | 45% |
| 2026 | 60% |
| 2027 | 75% |
| 2028 | 90% |
| 2029+ | conforme art. 17 — a ANEEL definirá a metodologia |

**2029 não é 100% automático.** A lei remete às condições que a agência definir. Trate
como incógnita configurável, não como valor conhecido.

Regimes por data de protocolo do pedido de acesso:

- **GD I** — protocolo até 06/01/2023. Mantém compensação integral, sem Fio B, até 2045.
- **GD II** — a maioria dos sistemas novos. Segue o cronograma acima. **É o caso de
  todo cliente novo do MVP.**
- **GD III** — minigeração acima de 500 kW em modalidades específicas. Fora do escopo.

Efeito prático a comunicar na proposta: a economia **cai ao longo dos anos**. A projeção
tem que mostrar isso. Esconder é o que a concorrência faz.

O cronograma inteiro vive em tabela configurável no admin, não em código.

## Regras de negócio do MVP

1. **Consumo compensável** = consumo médio mensal − custo de disponibilidade da ligação.
   Nunca dimensionar sobre o consumo bruto.

2. **Kit litoral.** Município costeiro dentro do raio configurado seleciona
   automaticamente equipamento e estrutura resistentes a névoa salina, com preço próprio.
   A proposta explica ao cliente por que o preço difere. Municípios candidatos: Vitória,
   Vila Velha, Serra, Cariacica (parte), Guarapari, Anchieta, Piúma, Marataízes,
   Presidente Kennedy, Aracruz, Fundão, Linhares, São Mateus, Conceição da Barra.
   A lista final e o raio são configuráveis.

3. **Cobertura parcial.** Quando a área informada não comporta o sistema ideal, mostrar
   explicitamente o percentual de cobertura alcançável, em vez de silenciosamente
   reduzir o sistema.

4. **Roteamento para humano.** Grupo A, potência acima do limite configurado, ou conta
   com geração já existente saem do fluxo automático e viram lead para engenheiro.

5. **Rural é diferente.** Trifásico com custo de disponibilidade de 100 kWh, consumo
   sazonal concentrado em irrigação e secagem de café, e modalidades tarifárias com
   desconto em horário noturno que alteram o cálculo. Tratar rural como residencial
   produz número errado. No MVP, subgrupo B2 usa parâmetros próprios.

6. **Bandeira tarifária não entra na projeção de 25 anos.** Muda todo mês e vira ruído
   que infla a promessa. Usar tarifa sem bandeira e declarar isso na proposta.

## Fórmulas

Notação: valores mensais salvo indicação.

### Dimensionamento

```
consumoCompensavel   = max(0, consumoMedioMensal - custoDisponibilidade)
consumoDiario        = consumoCompensavel / 30
geracaoPorKwpDia     = hspMedio * PR * fatorOrientacao
potenciaNecessariaKwp = consumoDiario / geracaoPorKwpDia
quantidadeModulos    = ceil(potenciaNecessariaKwp * 1000 / potenciaModuloW)
potenciaInstaladaKwp = quantidadeModulos * potenciaModuloW / 1000
areaNecessariaM2     = quantidadeModulos * areaModuloM2
```

`fatorOrientacao` sai de uma tabela configurável azimute × inclinação. No MVP começa
como um único fator informado; refinar depois.

O inversor é escolhido do catálogo respeitando o oversizing máximo configurado.

### Geração e economia

```
geracaoMensal[m]     = potenciaInstaladaKwp * hsp[m] * 30 * PR * fatorOrientacao
energiaCompensada[m] = min(geracaoMensal[m], consumoCompensavel[m])

custoFioB[m]         = energiaCompensada[m] * valorFioBPorKwh * percentualFioB(ano)
economiaBruta[m]     = energiaCompensada[m] * tarifaCheia
economiaLiquida[m]   = economiaBruta[m] - custoFioB[m]

faturaResidual[m]    = custoDisponibilidade * tarifaCheia
                     + max(0, consumo[m] - geracaoMensal[m]) * tarifaCheia
                     + custoFioB[m]
```

`tarifaCheia` inclui TE + TUSD e tributos, conforme extraído da conta ou da tabela
configurada. Documente no código exatamente qual composição está sendo usada.

### Financeiro, horizonte de 25 anos

```
geracaoAno[n]  = geracaoAno[1] * (1 - degradacaoAnual)^(n-1)
tarifaAno[n]   = tarifa[1] * (1 + inflacaoTarifaria)^(n-1)
percentualFioB depende do ano-calendário, pela tabela configurada
fluxo[n]       = economiaLiquidaAnual[n]
fluxo[0]       = -capex

paybackSimples     = primeiro n com soma(fluxo[1..n]) >= capex
paybackDescontado  = idem, descontando fluxo[n] por (1 + taxaDesconto)^n
VPL                = soma(fluxo[n] / (1 + taxaDesconto)^n) - capex
TIR                = taxa que zera o VPL  (bisseção ou Newton; documente a tolerância)
```

Todos os parâmetros em itálico conceitual acima — PR, degradação, inflação tarifária,
taxa de desconto, oversizing, horizonte — são configuráveis. As fórmulas ficam em código.

## Fontes oficiais

- Lei 14.300/2022, art. 17, 26 e 27
- REN ANEEL 1.059/2023
- Dados abertos ANEEL: `dadosabertos.aneel.gov.br` — datasets "Tarifas de aplicação das
  distribuidoras" e "Componentes Tarifárias"
- Atlas Brasileiro de Energia Solar (INPE / LABREN) e SunData (CRESESB) para HSP
- ARSP, agência reguladora estadual do ES

## Premissas e suas origens

Todo parâmetro do cálculo carrega uma `Origem`, que determina se ele pode ir a cliente
real. O mecanismo está descrito no doc 05; esta é a classificação inicial.

| Premissa | Origem | Nota |
|---|---|---|
| Cronograma do Fio B | `Lei` | art. 27 da Lei 14.300 |
| Custo de disponibilidade (30/50/100 kWh) | `Lei` | por tipo de ligação |
| Regime GD I / II / III | `Lei` | data de protocolo do pedido de acesso |
| HSP por município | `FontePublica` | Atlas Brasileiro / CRESESB |
| Tarifas TE, TUSD e Fio B | `FontePublica` | dados abertos ANEEL, com resolução homologatória |
| Distância do mar por município | `FontePublica` | geodado |
| Performance ratio | `Provisorio` | assumir 0,78 até validar |
| Degradação anual do módulo | `Provisorio` | assumir 0,5% a.a. |
| Inflação tarifária projetada | `Provisorio` | premissa comercial, decisão do dono |
| Taxa de desconto do VPL | `Provisorio` | premissa comercial |
| Oversizing máximo | `Provisorio` | prática do instalador |
| Faixas de preço e margem | `Provisorio` | tabela da empresa |
| Raio e lista do kit litoral | `Provisorio` | critério do dono |
| Limite de kWp para roteamento humano | `Provisorio` | critério do dono |
| Fator de orientação padrão | `Provisorio` | refinar com tabela azimute × inclinação |

Padrão que se repete: **o que é lei ou dado oficial já está resolvido; o que é opinião
comercial é provisório.** Por isso o projeto pode ser construído antes de falar com a
empresa — a incerteza está concentrada em parâmetros isolados, não nas fórmulas.

Cada `Provisorio` precisa de justificativa escrita. Só a validação no portão G1 promove
uma premissa.

## Armadilhas conhecidas

- Percentual do Fio B aplicado sobre a tarifa cheia em vez de sobre o componente Fio B
- Ignorar o custo de disponibilidade e prometer economia de 100%
- Usar consumo de um único mês em vez da média de 12
- Tratar rural como residencial
- Confundir kWp com kWh
- Modelar 2029 como 100% de Fio B
- Usar `double` para dinheiro
