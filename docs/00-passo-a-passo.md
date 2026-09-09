# 00 — Passo a passo completo

Guia único, do zero até a última tarefa. Os documentos 05 e 06 têm o detalhe e o
porquê; este tem a sequência literal.

---

# Parte 0 — Pré-requisitos

```bash
dotnet --version      # LTS atual
node --version        # 20+
psql --version        # ou Docker
git --version
```

Postgres local via Docker, se preferir não instalar:

```bash
docker run -d --name solares-db \
  -e POSTGRES_PASSWORD=dev -e POSTGRES_DB=solares \
  -p 5432:5432 postgres:16
```

---

# Parte 1 — Claude Code

```bash
npm install -g @anthropic-ai/claude-code
claude --version
```

Outras formas de instalar em `https://docs.claude.com/en/docs/claude-code/overview`.

---

# Parte 2 — Repositório

## 2.1 Criar e commitar o planejamento

```bash
mkdir solares && cd solares
git init

# copie CLAUDE.md, LEIA-ME.md, docs/ e .claude/ para a raiz

git add .
git commit -m "docs: planejamento inicial"
```

## 2.2 Renomear o projeto

Se a empresa usa outro nome, troque agora — antes de existir código:

```bash
grep -rl "SolarES" . | xargs sed -i 's/SolarES/NomeReal/g'
git commit -am "docs: renomeia projeto"
```

## 2.3 .gitignore

```bash
dotnet new gitignore
printf '\nfixtures/brutas/\n*.pdf\n.env\n' >> .gitignore
git commit -am "chore: gitignore"
```

`fixtures/brutas/` é a pasta onde contas ainda não anonimizadas podem cair. Nunca vai
para o git.

## 2.4 Primeira sessão

```bash
claude
```

Dentro dela:

```
/context
```

Confirme que `CLAUDE.md` aparece entre os arquivos de memória. Se não aparecer, ele não
está na raiz.

> **Não rode `/init`.** Ele gera um `CLAUDE.md` automático e sobrescreveria o nosso.

## 2.5 Permissões

Ainda na sessão:

> Leia `docs/06-fluxo-com-claude-code.md`, seção de permissões. Crie o
> `.claude/settings.json` com a allow-list descrita, validando a sintaxe contra a
> documentação atual do Claude Code. `git push` fica fora da allow-list.

```bash
git add .claude/settings.json && git commit -m "chore: permissões do Claude Code"
```

## 2.6 Hook de proteção de dado pessoal

**Antes de pedir qualquer conta à empresa.** É o único erro irreversível do projeto.

> Leia `docs/06-fluxo-com-claude-code.md`, seção de hooks. Crie um hook que bloqueie
> escrita em `fixtures/` de qualquer arquivo contendo padrão de CPF, nome de titular,
> endereço ou número de UC. Valide a sintaxe contra a documentação atual. Escreva um
> teste manual que eu possa rodar para confirmar que o bloqueio funciona.

Teste você mesmo antes de confiar. Depois:

```bash
git add .claude && git commit -m "chore: hook de proteção de dado pessoal"
```

## 2.7 Comando customizado

> Crie `.claude/commands/nova-tarefa.md` conforme descrito no `docs/06`.

Com isso, cada tarefa abaixo pode ser aberta com `/nova-tarefa T04`.

---

# Parte 3 — O ritual de cada tarefa

Vale para todas. As seções seguintes só variam o prompt de abertura.

```bash
git checkout main && git pull
git checkout -b feat/T04-scaffold
claude
```

Na sessão:

1. `Shift+Tab` até entrar em **plan mode**
2. Cole o prompt de abertura da tarefa
3. Leia o plano proposto. Corrija ou aprove
4. Saia do plan mode; peça o **teste primeiro**, revise, depois a implementação
5. Peça prova: *"Rode a suíte e me mostre. Depois me questione: o que pode ter quebrado e não está coberto?"*
6. Feche:

```bash
dotnet test
git add . && git commit -m "feat: <descrição>"
git checkout main && git merge feat/T04-scaffold
```

7. `/clear` antes da próxima tarefa, ou saia com `Ctrl+C` duas vezes

**Se precisou de `/compact`, a tarefa era grande demais.** Quebre a próxima.

---

# Parte 4 — Fase 1: Fundação

Zero dependência da empresa.

## T01 — Scaffold

```bash
git checkout -b feat/T01-scaffold
```

> Leia `CLAUDE.md` e `docs/03-arquitetura.md`. Execute a T01 do `docs/05`. Crie a solução
> com os quatro projetos, `Directory.Build.props`, analisadores e os projetos de teste.
> `SolarES.Dominio` não pode referenciar nada de I/O — proponha como garantir isso.

## T02 — Tipos de domínio

```bash
git checkout -b feat/T02-tipos-dominio
```

> Execute a T02. Crie os tipos do doc 04 e o `Premissa<T>` com `Valor`, `Origem` e
> `Justificativa`. Antes: me explique com suas palavras por que `Origem` é obrigatória.
> Nenhum `double` em campo financeiro.

## T03 — Baseline de premissas

```bash
git checkout -b feat/T03-premissas
```

> Execute a T03. Use a tabela "Premissas e suas origens" do `docs/02` como fonte. Cada
> `Provisorio` precisa de justificativa. Escreva o teste que falha se alguma premissa
> ficar sem origem.

## T04 — EF Core e migration

```bash
git checkout -b feat/T04-persistencia
```

> Execute a T04. Implemente o `DbContext` e as tabelas do `docs/04`, com `TenantId`
> reservado e as precisões `decimal` indicadas. Me mostre o SQL da migration antes de
> aplicar.

**Revise o SQL você mesmo.** Migration é um dos itens que o doc 06 marca como não
delegável.

## T05 — ConfiguracaoVersao

```bash
git checkout -b feat/T05-configuracao-versionada
```

> Execute a T05. Antes do código, me explique por que versão publicada precisa ser
> imutável e o que quebra se não for. Depois implemente rascunho, publicação e resolução
> da versão ativa, com testes das invariantes do `docs/04`.

## T06 — Seed de municípios

```bash
git checkout -b feat/T06-seed-municipios
```

> Execute a T06. Os 78 municípios do ES com HSP mensal, coordenadas e distância do mar.
> Antes de escrever, me diga de onde vai tirar os valores de HSP e como vou conferir a
> origem depois. Marque como `FontePublica` com a referência.

## T07 — Seed de tarifas

```bash
git checkout -b feat/T07-seed-tarifas
```

> Execute a T07. EDP ES e ELFSM a partir dos dados abertos da ANEEL. `ValorFioBPorKwh`
> separado da TUSD, com a resolução homologatória e a vigência registradas.

---

# Parte 5 — Fase 2: Motor

Fórmulas no `docs/02`. **Toda tarefa aqui começa com o Claude reformulando a regra.**

## T08 — Dimensionamento

```bash
git checkout -b feat/T08-dimensionamento
```

> Leia `docs/02`. Execute a T08. Antes de qualquer código, me explique com suas palavras
> o que é custo de disponibilidade e por que ele não é compensável. Depois: teste
> primeiro, cobrindo as três ligações e o caso de consumo abaixo do mínimo.

## T09 — Geração mensal

```bash
git checkout -b feat/T09-geracao
```

> Execute a T09. Geração mês a mês a partir do HSP sazonal do município e do PR, e
> energia compensada por mês.

## T10 — Fio B

```bash
git checkout -b feat/T10-fio-b
```

> Execute a T10. Antes do código: me explique sobre qual componente o percentual da Lei
> 14.300 incide, e o que acontece a partir de 2029. Escreva um teste que falha se o
> percentual for aplicado sobre a tarifa cheia.

Esta é a tarefa onde código errado parece mais certo. Leia o diff com atenção.

## T11 — Projeção de 25 anos

```bash
git checkout -b feat/T11-projecao
```

> Execute a T11. Degradação, inflação tarifária e Fio B crescente. O teste precisa
> afirmar explicitamente que a economia real decresce no início do horizonte.

## T12 — Financeiro

```bash
git checkout -b feat/T12-financeiro
```

> Execute a T12. Payback simples e descontado, VPL e TIR. Valide a TIR contra um caso
> conhecido e documente tolerância e método de convergência.

## T13 — Regras de contorno

```bash
git checkout -b feat/T13-contorno
```

> Execute a T13. Kit litoral, cobertura parcial e roteamento para humano, conforme as
> regras do `docs/02`. Área insuficiente devolve `CoberturaPercentual` abaixo de 100 —
> nunca reduz o sistema em silêncio.

## T14 — Suíte de invariantes

```bash
git checkout -b feat/T14-invariantes
```

> Execute a T14. Implemente as seis invariantes listadas no `docs/05`, sobre entradas
> geradas aleatoriamente dentro de faixas realistas. Me mostre quantos casos rodam e
> quanto tempo leva.

**Marco.** O motor está fechado e não produz absurdo. Rode a suíte inteira e faça uma
tag:

```bash
dotnet test
git tag motor-v1
```

---

# Parte 6 — Fase 3: Admin e proposta

## T15 — API do admin

```bash
git checkout -b feat/T15-api-admin
```

> Execute a T15. CRUDs de catálogo, faixas de preço, tarifas, municípios e premissas,
> com validação de entrada e OpenAPI gerado.

## T16 — Autenticação

```bash
git checkout -b feat/T16-auth
```

> Execute a T16. Identity + JWT com os perfis Dono, Vendedor e Engenheiro. Vendedor não
> publica configuração.

## T17 — Workspace Angular

```bash
git checkout -b feat/T17-workspace-web
```

> Execute a T17. Workspace com `landing` (SSR), `admin` (SPA) e `shared`, com client
> gerado do OpenAPI.

## T18 — Telas do admin

```bash
git checkout -b feat/T18-telas-admin
```

> Execute a T18. Catálogo, preços, premissas, tarifas, publicação com diff contra a
> versão ativa, e o painel de premissas provisórias com justificativa e responsável.

O painel é a sua ferramenta de conversa com o dono. Vale caprichar.

## T19 — Simulação interna

```bash
git checkout -b feat/T19-simulacao-manual
```

> Execute a T19. Formulário de entrada manual, resultado na tela e listagem de
> simulações. Este é o caminho principal do produto, não um fallback.

## T20 — PDF

```bash
git checkout -b feat/T20-pdf
```

> Execute a T20. Antes: confirme a licença Community do QuestPDF e me avise se houver
> restrição. A marca d'água de calibração pendente aparece enquanto existir premissa
> `Provisorio` ativa, com teste cobrindo os dois casos.

## T21 — Fila

```bash
git checkout -b feat/T21-fila
```

> Execute a T21. Hangfire para geração de PDF, com reprocessamento de job falho.

**Marco.** Mostre para o dono. É aqui que a conversa sobre as premissas fica fácil.

```bash
git tag nucleo-v1
```

---

# Parte 7 — G1: Portão de calibração

Roda quando o material chegar. Pode ser agora ou daqui a dois meses.

## G1.1 — Fixtures

```bash
git checkout -b chore/G1-fixtures
```

Anonimize as contas **fora** do repositório primeiro. Só então:

> Monte `fixtures/contas/` a partir dos pares em `fixtures/brutas/`. Nenhum nome, CPF,
> endereço ou número de UC pode sair no resultado. Registre a matriz de variação em
> `fixtures/README.md`, indicando o que ficou descoberto.

## G1.2 — Suíte de regressão

```bash
git checkout -b feat/G1-regressao
```

> Implemente a suíte de regressão contra as fixtures. O relatório mostra desvio **caso a
> caso**, com os piores primeiro. Não quero média.

## G1.3 — Resolver divergências

Uma conversa com o dono por divergência. Ou o motor corrige, ou a premissa dele vira
valor validado.

> Não ajuste valor esperado para o teste passar. Reporte o desvio e me explique a
> hipótese sobre a causa.

## G1.4 — Casos sintéticos

> Gere 10 casos cobrindo os buracos da matriz de variação, calcule pelo motor e monte um
> documento legível para o dono aprovar por sim/não.

## G1.5 — Zerar as provisórias

> Promova as premissas validadas. Confirme que o painel está vazio e que a marca d'água
> some. Rode a suíte inteira.

```bash
git tag calibrado-v1
```

**A partir daqui o sistema pode emitir proposta para cliente real.**

---

# Parte 8 — Fase 4: Landing

## T22 a T28

```bash
git checkout -b feat/T22-landing-ssr
```

> Execute a T22. App `landing` com SSR e orçamento de bundle definido. Me diga qual
> orçamento você propõe e por quê.

```bash
git checkout -b feat/T23-fluxo-publico
```

> Execute a T23. Fluxo completo: entrada manual, resultado, captura de lead no envio do
> PDF. Guarde o anexo se o visitante oferecer, mesmo sem saber lê-lo ainda.

```bash
git checkout -b feat/T24-lgpd
```

> Execute a T24. Consentimento antes de qualquer coleta, com data registrada.

Revise este diff pessoalmente. Dado pessoal é item não delegável.

```bash
git checkout -b feat/T25-envio
git checkout -b feat/T26-vencimento
git checkout -b feat/T27-telemetria
git checkout -b feat/T28-endurecimento
```

> Execute a T25 / T26 / T27 / T28 conforme o `docs/05`.

---

# Parte 9 — Bloco E: Extração (condicional)

Só depois da landing no ar, quando contas reais já estiverem chegando pelo funil com
consentimento.

```bash
git checkout -b feat/T29-parser-edp
```

> Execute a T29. Parser determinístico da conta da EDP ES. Ele precisa **falhar
> explicitamente** quando o layout não bater, nunca adivinhar.

```bash
git checkout -b feat/T30-fallback-visao
git checkout -b feat/T31-confirmacao
```

> Execute a T30 / T31 conforme o `docs/05`.

---

# Parte 10 — Quando dá errado

**Claude foi por um caminho ruim.** `Esc` para interromper. `Esc` `Esc` ou `/rewind`
para voltar ao checkpoint. Reformule o pedido em vez de tentar consertar por cima.

**Sessão ficou confusa.** `/clear` e recomece a tarefa. Contexto poluído produz código
pior que contexto vazio.

**Teste de regressão falhou depois de um refactor.** Investigue o desvio. Não relaxe a
asserção, não ajuste o valor esperado. Está no `CLAUDE.md` por um motivo.

**Você corrigiu a mesma coisa duas vezes.** Vira linha no `CLAUDE.md` ou numa rule. É a
diferença entre desenvolvimento assistido produtivo e repetir contexto todo dia.

**Precisou de `/compact` no meio de uma tarefa.** A tarefa era grande. Quebre e refaça.

---

# Resumo em uma tela

```
Parte 0-2   pré-requisitos, instalação, repositório, hook de proteção
T01–T07     fundação: scaffold, tipos, premissas, banco, seeds públicos
T08–T14     motor: dimensionamento, Fio B, projeção, financeiro, invariantes
T15–T21     admin e proposta: API, auth, Angular, telas, PDF, fila
G1          calibração com o dono — quando o material chegar
T22–T28     landing pública
T29–T31     extração automática, condicional
```

Ordem obrigatória: fundação antes de motor, motor antes de admin, **G1 antes de qualquer
proposta a cliente real.** O resto tem folga.
