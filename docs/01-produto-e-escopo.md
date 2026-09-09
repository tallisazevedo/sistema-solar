# 01 — Produto e escopo

## Tese

Sites de integradora solar são folhetos com formulário. Preço, dimensionamento e prazo
ficam atrás de "fale com um consultor", e o visitante sai sabendo menos do que entrou.

Este projeto entrega de graça o que o mercado esconde: uma proposta preliminar real,
personalizada, em minutos, sem falar com ninguém. O site deixa de ser vitrine e vira
ferramenta.

Segundo pilar: nada no cálculo é hardcoded. O dono da empresa ajusta preço, catálogo,
tarifa e premissas por um admin, e cada alteração gera uma versão rastreável.

## Contexto

- Região: Espírito Santo
- Distribuidoras: EDP ES (70 dos 78 municípios) e ELFSM / Santa Maria (o restante,
  região de Colatina)
- Público: misto — residencial, comercial de pequeno porte e rural
- Empresa da família já opera; existe base de clientes, tabela de preços e processo
  de homologação estabelecidos

## Como o projeto começa

O projeto não espera a empresa. Nenhuma tarefa do plano de execução tem a coleta como
pré-requisito.

O material do dono serve para uma coisa só: confirmar que os números batem com o que os
vendedores produzem hoje. Isso é calibração, não construção. Então o desenvolvimento
começa com premissas provisórias declaradas, e a calibração vira um portão (G1 no doc 05)
que roda quando o material chegar — semana 3 ou semana 12, tanto faz.

O que torna isso seguro é o mecanismo de origem de premissa. Todo parâmetro do cálculo é
marcado como `Lei`, `FontePublica` ou `Provisorio`. Enquanto existir premissa
`Provisorio`, o PDF sai com marca d'água de calibração pendente e a proposta é interna.
Nenhuma proposta vai a cliente real antes do portão. É regra de código, verificada em
teste, não disciplina.

Dois seeds já eliminam boa parte da incerteza sem falar com ninguém: HSP dos 78
municípios capixabas, do Atlas Brasileiro de Energia Solar, e tarifas da EDP ES e da
ELFSM, dos dados abertos da ANEEL. Fontes públicas e verificáveis.

### Trilha paralela de coleta

Quando for possível, e em qualquer ordem:

1. **Conversa de 15 minutos** com o dono sobre interesse no projeto. É a única coisa que
   vale fazer antes do primeiro commit, e o risco que ela elimina não é técnico.
2. **15 pares de conta + proposta**, escolhidos por cobertura de variação: as três
   ligações, os três subgrupos, litoral / serra / norte, faixas de consumo distintas.
   Dez pares completos valem mais que quinze contas soltas.
3. **Uma hora gravada** com ele montando uma proposta do zero em voz alta.
4. **A tabela de preços real.**

O painel de premissas provisórias no admin é a ferramenta que torna esse pedido fácil:
em vez de pedir contas abstratas, você mostra a tela e pergunta se aqueles seis números
estão certos.

Custo do gabarito reduzido: fixture sintética valida regressão, não correção. A
compensação é a suíte de invariantes (T14), a revisão caso a caso no G1, e dez casos
sintéticos aprovados por ele.

## Escopo do MVP

### Dentro

- Admin com catálogo de equipamentos, precificação, premissas técnicas, tarifas, HSP
  por município e configuração versionada
- Motor de cálculo como domínio puro, coberto por testes contra as contas reais
- Entrada manual dos dados da conta como caminho principal, não como fallback
- Proposta em PDF com validade explícita
- Landing pública com o simulador e captura de lead
- Grupo B apenas: residencial, comercial pequeno, rural

### Fora, e por escrito

- Simulador de telhado por satélite
- Portal de acompanhamento pós-venda
- Integração automática com dados abertos da ANEEL
- Simulação de impacto no admin antes de publicar configuração
- Financiamento
- Grupo A automatizado (média tensão, demanda contratada, ponta/fora-ponta)
- Dashboard público de geração
- Páginas por município para SEO
- Multi-tenant
- Parser da ELFSM

**Condicional:** extração automática da conta da EDP ES (Bloco E do doc 05). Executa
depois do M1 estabilizado, e só se as 15 contas bastarem para validar o parser. Se não
bastarem, o bloco sai — a entrada manual já cobre o fluxo inteiro. Depois do M2 no ar,
contas reais chegam pelo próprio funil, com consentimento, e o bloco fica mais fácil de
fazer, não mais difícil.

Duas justificativas que valem registrar:

**ELFSM fica fora** porque atende 8 dos 78 municípios. O parser só da EDP ES cobre a
esmagadora maioria do volume, e a entrada manual atende o resto sem bloquear ninguém.

**Telhado por satélite fica fora** porque é a feature mais vistosa e a que menos altera
o resultado. Em residencial capixaba a área do telhado raramente é o gargalo. No MVP a
área é informada manualmente e usada só para checar se o sistema cabe.

## Marcos

### M1 — Ferramenta interna (~7 semanas)

Os vendedores param de usar Excel. Nenhum visitante externo vê nada.

Isso é deliberado: valida o motor contra dados reais, com usuários do seu lado, sem
exposição pública nem risco jurídico — e conquista o apoio interno antes de pedir
qualquer coisa à empresa.

**Pronto quando:** o portão G1 fechou (painel de premissas provisórias vazio, marca
d'água removida) e os vendedores geram 100% das propostas novas pelo sistema por duas
semanas seguidas sem voltar ao Excel.

O núcleo fica funcional antes do G1. O que o portão libera é a emissão de proposta para
cliente real.

### M2 — Landing pública (~4 semanas)

O mesmo motor exposto num funil público, mobile-first.

**Pronto quando:** uma pessoa de fora, no celular, em 4G, sai da home com o PDF no
e-mail em menos de cinco minutos.

## Métricas

**Internas (M1)**
- Tempo médio para gerar uma proposta, antes e depois
- Percentual de propostas emitidas pelo sistema
- Desvio do motor contra as propostas históricas

**Públicas (M2)**
- Taxa de conclusão do simulador
- Percentual de leads que anexaram a conta (proxy de qualidade do lead)
- Conversão de lead para visita técnica, comparada aos canais atuais

## Riscos

| Risco | Mitigação |
|---|---|
| Empresa demora a entregar o material | Não bloqueia. Constrói-se com premissas provisórias e calibra-se no G1. |
| Empresa não estar engajada com o projeto | Conversa de 15 min antes do primeiro commit |
| Proposta errada chegar a cliente antes da calibração | Marca d'água e bloqueio por premissa `Provisorio`, em código |
| Gabarito pequeno esconder erro numa média | G1 avalia caso a caso, nunca média |
| Escopo: vontade de construir o telhado 3D | Está no backlog. Não olhe até M2 rodar. |
| Motor errado passando despercebido | Suíte de regressão contra gabarito, rodando no CI |
| OCR gerando proposta errada | Tela de confirmação editável obrigatória, sempre |
| LGPD na conta de luz | Consentimento antes do upload, descarte da imagem após extração |
| Vendedor rejeitar a ferramenta | M1 é interno justamente para descobrir isso cedo |

## Mínimo jurídico do MVP

- Disclaimer de estimativa preliminar sujeita a visita técnica, em toda proposta
- Validade explícita na proposta (`ValidaAte`)
- Consentimento LGPD antes do upload da conta
- Descarte da imagem original após a extração; guardar só os campos
- Validação humana antes de qualquer proposta virar contrato

## Backlog pós-MVP, em ordem

1. Simulação de impacto no admin antes de publicar configuração
2. Parser da ELFSM
3. Integração com dados abertos da ANEEL, gerando alertas de revisão
4. Portal de acompanhamento pós-venda, com acesso sem senha
5. Simulador de telhado por satélite
6. Financiamento comparado
7. Páginas por município e dashboard de geração
