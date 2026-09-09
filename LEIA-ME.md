# Pacote de planejamento — SolarES

Plataforma de proposta instantânea de energia solar para uma integradora do Espírito
Santo. Documentação de planejamento para desenvolvimento assistido por Claude Code.

Renomeie `SolarES` para o nome que a empresa usar antes de começar.

## Como usar

Copie tudo para a raiz de um repositório novo:

```bash
mkdir solares && cd solares
git init
# copie CLAUDE.md, docs/ e .claude/ para cá
git add . && git commit -m "docs: planejamento inicial"
claude
```

Na primeira sessão, rode `/context` e confirme que `CLAUDE.md` aparece entre os arquivos
de memória. **Não rode `/init`** — ele sobrescreveria o `CLAUDE.md` escrito à mão.

Este arquivo (`LEIA-ME.md`) é só um índice. Pode apagar depois de ler.

## Arquivos

| Arquivo | O que é |
|---|---|
| `CLAUDE.md` | Carregado em toda sessão do Claude Code. Comandos, layout, convenções, regras invioláveis. Mantenha enxuto. |
| `.claude/rules/dominio.md` | Regra por caminho: carrega só quando a sessão toca o domínio ou seus testes. |
| `docs/00-passo-a-passo.md` | **Comece por aqui na hora de executar.** Sequência literal do zero à última tarefa, com o comando e o prompt de cada passo. |
| `docs/01-produto-e-escopo.md` | Tese, contexto do ES, o que é MVP e o que não é, marcos, métricas, riscos. |
| `docs/02-dominio-e-glossario.md` | Glossário, Lei 14.300, regras de negócio, fórmulas, origem de cada premissa. **O documento mais importante.** |
| `docs/03-arquitetura.md` | Camadas, módulos, decisões registradas, o que não fazer. |
| `docs/04-modelo-de-dados.md` | Entidades, invariantes, índices, retenção e LGPD. |
| `docs/05-plano-de-execucao.md` | 31 tarefas em ordem, com critério de aceite. Uma tarefa por sessão. |
| `docs/06-fluxo-com-claude-code.md` | Instalação, comandos de terminal, ciclo por tarefa, permissões, hooks. |

## Por onde começar

1. Leia `docs/01` e `docs/02`. São os que carregam as decisões.
2. Revise o `CLAUDE.md` — ele tem convenções que eu escolhi por você (nomes de projeto,
   idioma no código). Mudar depois é caro.
3. Configure o hook que bloqueia dado pessoal em `fixtures/` (doc 06). Antes de pedir
   qualquer conta à empresa.
4. Siga o `docs/00-passo-a-passo.md` do início. Ele já embute os passos 2 e 3 acima.

## O que este plano assume

**O projeto não espera a empresa.** Nenhuma tarefa tem a coleta de contas como
pré-requisito. Todo parâmetro do cálculo é marcado como `Lei`, `FontePublica` ou
`Provisorio`; enquanto houver premissa `Provisorio` ativa, o PDF sai com marca d'água e
nenhuma proposta vai a cliente real. A calibração com o dono é o portão G1, que roda
quando o material chegar — semana 3 ou semana 12, sem alterar o plano.

A única coisa que vale fazer antes do primeiro commit é uma conversa de 15 minutos com o
dono sobre interesse no projeto. É sobre engajamento, não sobre arquivos.

## Verificar antes de codificar

Estes pontos podem ter mudado desde a redação:

- Cronograma do Fio B e a regra a partir de 2029 (ANEEL define pelo art. 17)
- Licença Community do QuestPDF frente ao faturamento da empresa
- Sintaxe atual de `settings.json` e hooks do Claude Code
