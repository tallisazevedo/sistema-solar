# 05 — Plano de execução

Cada tarefa é uma sessão de Claude Code e uma branch. Se não couber numa sessão sem
`/compact`, está grande demais — quebre.

## Princípio deste plano

**Nada aqui espera resposta da empresa.**

O gabarito da empresa serve para uma coisa só: confirmar que os números batem com o que
os vendedores produzem hoje. Isso afeta a calibração, não a construção.

Então o projeto começa hoje, com premissas provisórias declaradas, e a calibração vira
um portão (G1) que roda quando o material chegar — em qualquer momento. A coleta é uma
trilha paralela, nunca um bloqueio.

O que torna isso seguro é o mecanismo da seção seguinte. Sem ele, "construir agora e
calibrar depois" é como um projeto morre entregando número errado a cliente real.

---

## Mecanismo: premissa com origem

Todo parâmetro do cálculo carrega a origem do valor:

| Origem | Significado | Exemplos |
|---|---|---|
| `Lei` | vem da legislação, não se discute | cronograma do Fio B, custo de disponibilidade |
| `FontePublica` | dado oficial verificável | HSP do INPE, tarifas dos dados abertos da ANEEL |
| `Provisorio` | estimativa sua, a validar com a empresa | PR, oversizing, margem, critério de litoral |

Regras que decorrem disso:

1. Toda premissa `Provisorio` tem justificativa escrita e um responsável pela validação.
2. O admin exibe um painel com todas as premissas `Provisorio` ativas.
3. Enquanto existir premissa `Provisorio`, o PDF sai com marca d'água de calibração
   pendente e a proposta é marcada como interna.
4. **Nenhuma proposta vai a cliente real com premissa `Provisorio` ativa.** Isso é regra
   de código, verificada em teste, não disciplina.

Esse é o contrato que permite construir tudo antes de falar com o dono. Você constrói,
demonstra, e o sistema se recusa a mentir para um cliente enquanto não estiver calibrado.

---

## Trilha paralela — Coleta (sem data, sem bloqueio)

Execute quando for possível. Nenhuma tarefa das fases abaixo depende disso.

**C1 — Conversa de 15 minutos com o dono.** Sobre interesse no projeto, não sobre
arquivos. É a única coisa que vale fazer antes do primeiro commit, e o risco que ela
elimina não é técnico: é investir semanas num projeto que a empresa não quer.

**C2 — Pedido dos 15 pares.** Conta + proposta correspondente, escolhidos por variação:
as três ligações, os três subgrupos, litoral / serra / norte, faixas de consumo distintas.
Dez pares completos valem mais que quinze contas soltas.

**C3 — Uma hora gravada com o dono**, montando uma proposta do zero em voz alta. É onde
aparecem PR assumido, oversizing aceito, margem por faixa, critério de litoral, quando ele
recusa cliente. Cada premissa capturada aqui migra de `Provisorio` para valor validado.

**C4 — Fixtures.** `fixtures/contas/NN-entrada.json` e `NN-esperado.json`, anonimizadas.

> O hook que bloqueia commit de dado pessoal em `fixtures/` fica pronto **antes** de C2.
> Se as contas chegarem por WhatsApp num sábado e você jogar na pasta sem pensar, o
> estrago no histórico do git é permanente.

---

## Fase 1 — Fundação (semanas 1–2)

Zero dependência externa. Comece aqui, hoje.

**T01 — Scaffold**
Projetos, `Directory.Build.props`, analisadores, `dotnet test` verde vazio.
**Aceite:** build e test passam; `Dominio` sem referência a pacote de I/O.

**T02 — Tipos de domínio**
`ConfiguracaoCalculo`, `EntradaSimulacao`, `ResultadoSimulacao`, enums de ligação e
subgrupo, e o tipo `Premissa<T>` com `Valor`, `Origem`, `Justificativa`.
**Aceite:** compila; nenhum `double` em campo financeiro; `Origem` obrigatória em toda
premissa.

**T03 — Baseline de premissas**
Arquivo versionado com todas as premissas iniciais, cada uma classificada e justificada.
As de origem `Lei` saem do doc 02; as `Provisorio` são chutes explícitos.
**Aceite:** nenhuma premissa sem origem; o teste que garante isso existe e passa.

**T04 — EF Core e primeira migration**
`DbContext` e as tabelas do doc 04, com `TenantId` reservado.
**Aceite:** migration aplica em banco limpo; precisão `decimal` conforme o doc 04.

**T05 — ConfiguracaoVersao**
Rascunho, publicação, imutabilidade, resolução da versão ativa.
**Aceite:** alterar versão publicada falha; no máximo um rascunho e uma publicada.

**T06 — Seed de municípios**
78 municípios do ES com HSP mensal, coordenadas e distância do mar. Fonte: Atlas
Brasileiro de Energia Solar (INPE/LABREN) ou SunData (CRESESB).
**Aceite:** seed idempotente; todos os códigos IBGE presentes; origem `FontePublica`
registrada com a referência.

**T07 — Seed de tarifas**
EDP ES e ELFSM a partir dos dados abertos da ANEEL — datasets "Tarifas de aplicação das
distribuidoras" e "Componentes Tarifárias". `ValorFioBPorKwh` separado da TUSD.
**Aceite:** valores rastreáveis à resolução homologatória; vigências corretas.

> T06 e T07 são dados públicos e verificáveis. Não dependem da empresa e já eliminam duas
> das maiores incertezas do cálculo.

---

## Fase 2 — Motor (semanas 2–4)

As fórmulas vêm da lei e da física, ambas documentadas no doc 02. Nada aqui depende da
planilha do dono — só os parâmetros dependem, e eles já estão isolados como premissas.

**T08 — Dimensionamento**
Consumo compensável, custo de disponibilidade, potência necessária, seleção de módulo e
inversor com oversizing.
**Aceite:** testes nas três ligações; consumo abaixo do custo de disponibilidade retorna
zero compensável, nunca negativo.

**T09 — Geração mensal**
Geração por mês a partir de HSP e PR; energia compensada mês a mês.
**Aceite:** HSP sazonal produz variação entre meses.

**T10 — Fio B**
Percentual do cronograma sobre `ValorFioBPorKwh`, por ano-calendário.
**Aceite:** teste prova que incide sobre o componente e **não** sobre a tarifa cheia; ano
fora da tabela usa a regra de fallback, sem assumir 100%.

**T11 — Projeção de 25 anos**
Degradação, inflação tarifária, Fio B crescente.
**Aceite:** a economia anual decresce em termos reais no início do horizonte, afirmado
explicitamente em teste.

**T12 — Financeiro**
Payback simples e descontado, VPL, TIR.
**Aceite:** TIR validada contra caso conhecido; convergência e tolerância documentadas.

**T13 — Regras de contorno**
Kit litoral, cobertura parcial por área, roteamento para humano.
**Aceite:** município costeiro seleciona kit litoral com preço próprio; área insuficiente
devolve `CoberturaPercentual` abaixo de 100 em vez de reduzir em silêncio.

**T14 — Suíte de invariantes**

Substitui a suíte de regressão enquanto não há gabarito. Em vez de comparar com um
resultado conhecido, verifica propriedades que precisam valer para **qualquer** entrada:

- economia nunca atinge 100% da conta, em nenhuma combinação de parâmetros
- potência dimensionada cresce monotonicamente com o consumo
- economia anual decresce ao longo do horizonte, em termos reais
- custo do Fio B nunca supera a economia bruta
- nenhum resultado financeiro é NaN, infinito ou negativo onde não deveria
- consumo abaixo do custo de disponibilidade produz recomendação de não instalar

**Aceite:** a suíte roda sobre entradas geradas aleatoriamente dentro de faixas realistas
e nenhuma invariante quebra.

> Isso não prova que o número está certo. Prova que ele não está absurdo — e pega a maior
> parte dos erros de fórmula, que é o que você consegue fazer sem gabarito. A suíte
> continua valendo depois da calibração, junto com a de regressão.

---

## Fase 3 — Admin e proposta (semanas 4–7)

**T15 — API do admin**
CRUDs de catálogo, faixas de preço, tarifas, municípios, premissas.
**Aceite:** OpenAPI gerado; validação de entrada em todos os endpoints.

**T16 — Autenticação**
Identity + JWT, perfis Dono, Vendedor e Engenheiro.
**Aceite:** admin rejeita anônimo; vendedor não publica configuração.

**T17 — Workspace Angular**
`landing`, `admin` e `shared`; client gerado do OpenAPI.
**Aceite:** `npm run start:admin` sobe e autentica.

**T18 — Telas do admin**
Catálogo, preços, premissas, tarifas, publicação de versão com diff contra a ativa, e o
**painel de premissas provisórias** com justificativa e responsável.
**Aceite:** o dono abre o painel e vê exatamente o que ainda precisa validar.

> Esse painel é a sua ferramenta de conversa com a empresa. Em vez de pedir 15 contas
> abstratas, você mostra a tela e pergunta "esses seis números estão certos?". É um
> pedido muito mais fácil de atender.

**T19 — Simulação interna por entrada manual**
Formulário com 12 meses ou consumo médio, ligação, município, tipo e área de telhado.
Resultado na tela, listagem de simulações.
**Aceite:** um vendedor gera simulação completa sem tocar em Excel e sem anexar conta.

**T20 — PDF**
QuestPDF: dimensionamento, projeção com economia decrescente, disclaimer, validade.
**Aceite:** PDF íntegro; `ValidaAte` presente; **marca d'água de calibração pendente
enquanto houver premissa `Provisorio`**, com teste cobrindo os dois casos.

**T21 — Fila**
Hangfire para geração de PDF e, depois, extração.
**Aceite:** geração não bloqueia request; job falho é reprocessável.

**Fim da construção do núcleo.** O sistema está funcional e honesto sobre o próprio
estado de calibração.

---

## G1 — Portão de calibração

Roda quando o material da coleta chegar. Pode ser na semana 3 ou na semana 12 — o plano
não muda por causa disso.

**G1.1 — Suíte de regressão**
Roda as fixtures de C4 e reporta desvio **caso a caso**, nunca média. Com gabarito
pequeno, média esconde erro.

**G1.2 — Resolução das divergências**
Cada desvio é discutido individualmente com o dono. Ou o motor está errado e se corrige,
ou a premissa dele é diferente e vira valor validado — nunca "arredonda e segue".

**G1.3 — Casos sintéticos**
Dez casos cobrindo os buracos da matriz de variação, calculados pelo motor e aprovados
pelo dono por sim/não. Fecha em torno de 25 casos de gabarito.

**G1.4 — Zerar as provisórias**
Toda premissa `Provisorio` migra para validada, ou é substituída pelo valor dele.

**Aceite do portão:** o painel de premissas provisórias está vazio, a marca d'água some,
e propostas passam a poder ir a cliente real.

> **Nada a jusante deste portão vai a público sem ele.** A landing pode estar pronta e
> hospedada; ela não emite proposta definitiva antes de G1.

---

## Fase 4 — Landing pública

**T22 — App landing com SSR**, com orçamento de bundle definido.
**Aceite:** first contentful paint dentro da meta em 4G simulado.

**T23 — Fluxo público**
Entrada manual bem desenhada, resultado na tela, captura de lead no envio do PDF.
**Aceite:** fluxo completo no celular em menos de cinco minutos.

**T24 — LGPD**
Consentimento antes de qualquer coleta; descarte de imagem quando houver upload.
**Aceite:** consentimento registrado com data; nenhuma imagem original persistida.

**T25 — Envio** por e-mail e WhatsApp.
**Aceite:** entrega confirmada nos dois canais.

**T26 — Vencimento de proposta**, job que marca `Vencida` e notifica o vendedor.
**Aceite:** proposta expirada não pode ser aceita.

**T27 — Telemetria do funil.**
**Aceite:** as três métricas do doc 01 aparecem no admin.

**T28 — Endurecimento**: rate limit, limite de tamanho, erro com mensagem humana.
**Aceite:** uso abusivo é barrado sem derrubar o serviço.

---

## Bloco E — Extração automática (condicional)

Só depois da landing no ar. A partir daí, contas reais chegam pelo próprio funil, com
consentimento explícito, em volume crescente — o que resolve o problema de origem sem
depender do arquivo da empresa.

Anote desde o T23: **guarde o anexo quando o visitante oferecer**, mesmo que o sistema
ainda não saiba lê-lo. Retenção conforme o doc 04.

**T29 — Parser determinístico EDP ES.**
**Aceite:** acerta as amostras e **falha explicitamente** em vez de adivinhar.

**T30 — Fallback com modelo de visão**, schema fixo, server-side.
**Aceite:** chave de API nunca chega ao cliente.

**T31 — Tela de confirmação editável**, obrigatória antes de simular.
**Aceite:** não existe caminho no código que crie `Simulacao` sem confirmação.

---

## Ordem de teste

- Motor: invariantes desde o T14; regressão a partir do G1. Cobertura alta, sem exceção.
- Aplicação: integração nos casos de uso que gravam snapshot.
- API: contrato nos endpoints públicos.
- Frontend: componente nos formulários; e2e só no fluxo público completo.

Não persiga cobertura no admin. Persiga cobertura no motor.
