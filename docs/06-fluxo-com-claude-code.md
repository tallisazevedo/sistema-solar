# 06 — Fluxo de trabalho com Claude Code

## Configuração do repositório

```
CLAUDE.md                    carregado em toda sessão; mantenha enxuto
.claude/
  rules/
    dominio.md               regra por caminho, carrega só ao tocar no domínio
    frontend.md
  commands/
    nova-tarefa.md           comandos customizados
    revisar-motor.md
  settings.json              permissões, modelo, statusline
docs/                        planejamento; referenciado, não colado no CLAUDE.md
fixtures/                    contas anonimizadas e gabaritos
```

### CLAUDE.md enxuto

O `CLAUDE.md` é carregado por inteiro em toda sessão. Guarde nele só o que vale em
qualquer tarefa: comandos, layout, convenções, regras invioláveis. Procedimento de
vários passos ou instrução que só vale para uma parte do código vira **rule** por
caminho ou **skill**.

A tentação natural é colar o plano inteiro no `CLAUDE.md`. Não faça. Documento longo
dilui as poucas regras que realmente importam. Aponte para `docs/` e deixe o Claude ler
o que a tarefa exigir.

Prefira declarar o objetivo a enumerar as verificações. "O motor não depende de I/O" vale
mais que quinze checagens de como confirmar isso.

### Rules por caminho

`.claude/rules/dominio.md` carrega apenas quando a sessão toca `src/SolarES.Dominio/`.
Bom lugar para as armadilhas do doc 02 — custo de disponibilidade, Fio B sobre o
componente, `decimal` no lugar de `double`.

## Terminal, do zero ao primeiro commit

### Instalação

```bash
npm install -g @anthropic-ai/claude-code
claude --version
```

Existem outros instaladores além do npm; confira
`https://docs.claude.com/en/docs/claude-code/overview` para a opção da sua plataforma.

### Dia 1

```bash
mkdir solares && cd solares
git init

# copie CLAUDE.md, docs/ e .claude/ para a raiz
git add .
git commit -m "docs: planejamento inicial"

claude
```

Dentro da sessão, primeiro comando:

```
/context
```

Confirme que `CLAUDE.md` aparece entre os arquivos de memória. Se não aparecer, ele está
no lugar errado — tem que estar na raiz do repositório.

> **Não rode `/init` neste projeto.** Ele varre o repositório e gera um `CLAUDE.md`
> automático. O nosso foi escrito à mão, com decisões deliberadas, e seria sobrescrito ou
> misturado. `/init` serve para repositório existente sem documentação.

### Anatomia de uma sessão de tarefa

```bash
git checkout -b feat/T06-seed-municipios
claude
```

Na sessão:

**1. Entre em plan mode.** `Shift+Tab` cicla os modos de permissão; pare no plano. Claude
lê e propõe sem escrever nada.

**2. Prompt de abertura.** Aponte os documentos e a tarefa:

> Leia `docs/05-plano-de-execucao.md` e `docs/02-dominio-e-glossario.md`. Vamos executar a
> T06. Antes de qualquer código, me proponha o desenho e me diga onde você vai buscar os
> valores de HSP.

**3. Revise o plano.** Aprove ou corrija. Sair do plan mode libera as edições.

**4. Teste antes.** Peça o teste, revise, depois peça a implementação.

**5. Exija prova.**

> Rode a suíte e me mostre o resultado. Depois me questione: o que pode ter quebrado e
> não está coberto?

**6. Feche.**

```bash
dotnet test
git add . && git commit -m "feat: seed de HSP dos municípios do ES"
```

**7. Encerre a sessão.** `/clear` para começar limpo na próxima tarefa, ou `Ctrl+C` duas
vezes para sair.

### Teclas e comandos que valem decorar

| Atalho / comando | O que faz |
|---|---|
| `Shift+Tab` | cicla modos de permissão, incluindo plan mode |
| `Esc` | interrompe o que Claude está fazendo |
| `Esc` `Esc` ou `/rewind` | volta a um checkpoint anterior, sem depender de git |
| `/context` | mostra o que está carregado no contexto |
| `/compact` | resume a conversa; encare como sinal de tarefa grande demais |
| `/clear` | zera a conversa mantendo o `CLAUDE.md` |
| `/memory` | edita os arquivos de memória |

`/rewind` é local à sessão e não cobre alterações feitas via bash. Commit frequente
continua sendo a rede de segurança principal.

### Permissões

`.claude/settings.json` evita aprovar o mesmo comando trinta vezes por dia:

```json
{
  "permissions": {
    "allow": [
      "Bash(dotnet build:*)",
      "Bash(dotnet test:*)",
      "Bash(npm run:*)",
      "Bash(git status:*)",
      "Bash(git diff:*)"
    ],
    "deny": [
      "Bash(git push:*)",
      "Bash(dotnet ef database drop:*)"
    ]
  }
}
```

Confira a sintaxe atual na documentação de settings antes de commitar — o esquema evolui.
Mantenha `git push` fora da allow-list: você quer revisar o diff antes de publicar.

### Comando customizado

`.claude/commands/nova-tarefa.md`:

```markdown
---
description: Inicia uma tarefa do plano de execução em plan mode
---

Leia `docs/05-plano-de-execucao.md` e `docs/02-dominio-e-glossario.md`.

Vamos executar a tarefa $ARGUMENTS.

Antes de escrever qualquer código:
1. Reformule com suas palavras a regra de domínio envolvida
2. Proponha o desenho
3. Liste o que o critério de aceite exige provar
```

Uso: `/nova-tarefa T06`.

### Hooks

`CLAUDE.md` sugere; hook obriga. Dois que se pagam cedo:

- Hook de pré-commit rodando `dotnet test tests/SolarES.Dominio.Tests`
- Hook que bloqueia escrita em `fixtures/` de arquivo com padrão de CPF ou com os campos
  de identificação da conta

O segundo fica pronto **antes** de pedir qualquer conta à empresa. É o único erro deste
projeto que você não desfaz.

### Trabalhar em paralelo

Quando uma sessão longa estiver rodando e você quiser adiantar outra coisa:

```bash
git worktree add ../solares-T07 -b feat/T07-seed-tarifas
cd ../solares-T07 && claude
```

Cada worktree é um diretório isolado com sua própria sessão. Evita duas sessões
disputando os mesmos arquivos.

## Ciclo por tarefa

Uma tarefa do doc 05 por sessão, uma branch por tarefa.

**1. Plan mode primeiro.** `Shift+Tab` para entrar. Claude lê, explora e propõe sem
escrever nada. Você aprova antes de qualquer edição. É o maior ganho de confiabilidade
disponível e custa um atalho de teclado.

**2. Abra com o contexto certo.** Um bom início de sessão:

> Leia `docs/05-plano-de-execucao.md` e `docs/02-dominio-e-glossario.md`. Vamos executar
> a T05. Antes de escrever código, me proponha o desenho e me diga o que você entendeu
> por "o percentual incide sobre o componente Fio B".

Fazer o Claude reformular a regra de domínio antes de codar pega mal-entendido cedo.
Neste projeto especificamente, é onde os erros caros nascem.

**3. Teste antes da implementação.** Peça o teste, revise o teste, depois peça a
implementação. Num projeto onde o correto é definido por um gabarito externo, teste
escrito depois tende a confirmar o que o código faz, não o que a regra exige.

**4. Exija prova.** "Prove que isso está certo rodando a suíte de regressão e me mostre
o desvio por caso." Depois: "me questione sobre essas mudanças — o que pode ter quebrado
e não está coberto?"

**5. Feche a sessão.** Rode a suíte, faça o commit em Conventional Commits, e atualize
o `CLAUDE.md` se a tarefa revelou uma convenção nova.

## Subagentes

Use para leitura pesada que não precisa entrar no seu contexto principal: mapear onde
uma regra está usada, ler documentação externa, varrer o repositório em busca de um
padrão. O subagente devolve só a conclusão, e sua conversa principal fica limpa.

Regra prática: se a tarefa é "descubra X", subagente. Se é "mude X", contexto principal.

## Contexto

- `/context` para ver o que está carregado
- `/compact` quando a sessão ficar longa — mas encarar `/compact` como sinal de que a
  tarefa era grande demais
- `/rewind` ou `Esc` duas vezes desfaz edições sem depender de git

Preferir sessão curta e focada a sessão longa e heroica. Tarefa bem cortada no doc 05
é o que torna isso possível.

## O que nunca delegar sem revisar linha a linha

Esses são os pontos onde código plausível e errado passa despercebido e custa caro:

- **Fórmulas do doc 02.** Especialmente Fio B e custo de disponibilidade. Código
  incorreto aqui compila, roda, produz número bonito e mente para o cliente.
- **Migrations.** Confira o SQL gerado antes de aplicar. Nunca edite migration aplicada.
- **Qualquer coisa que toque dado pessoal.** Descarte de imagem, mascaramento,
  consentimento. Revisar pessoalmente.
- **Textos da proposta.** Disclaimer e validade têm efeito jurídico.
- **Ajuste em teste que quebrou.** Se um teste de regressão falha, a resposta é
  investigar o desvio — nunca relaxar a asserção. Deixe isso explícito na sessão,
  porque "fazer o teste passar" é uma leitura razoável de uma instrução ambígua.

## Comandos customizados úteis

**`/nova-tarefa`** — recebe o código da tarefa, lê o doc 05 e o doc 02, entra em plan
mode e propõe o desenho com critério de aceite.

**`/revisar-motor`** — roda a suíte de regressão e apresenta o desvio por fixture, com
os piores casos primeiro.

**`/checar-dominio`** — verifica que `SolarES.Dominio` não ganhou dependência de I/O e
que nenhum `double` apareceu em campo financeiro.

## Hooks

`CLAUDE.md` sugere; hook obriga. Dois que se pagam cedo:

- Hook de pré-commit rodando `dotnet test tests/SolarES.Dominio.Tests`
- Hook bloqueando commit de arquivo dentro de `fixtures/` que contenha padrão de CPF ou
  os campos de identificação da conta

O segundo é barato e evita o pior erro possível neste projeto: dado pessoal de cliente
real entrando no histórico do git.

## Ritmo sugerido

Uma tarefa por sessão, uma sessão por dia útil, revisão do diff antes do merge. Trinta
tarefas no doc 05, dez semanas de plano. Sobra folga para o que sempre aparece.

O gargalo não vai ser velocidade de escrita de código. Vai ser você validar que o número
que sai está certo — e é exatamente por isso que T00 e T09 vêm antes de tudo.
