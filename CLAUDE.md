# SolarES

Plataforma de proposta instantânea de energia solar para uma integradora que atua no
Espírito Santo. O cliente envia a conta de luz, o sistema calcula o dimensionamento e
o retorno financeiro, e devolve uma proposta em PDF em minutos. Todos os parâmetros do
cálculo são configuráveis por um admin, sem deploy.

## Stack

- Backend: .NET (LTS atual), monolito modular, EF Core + Npgsql
- Banco: PostgreSQL, com uso deliberado de `jsonb` para snapshots
- Frontend: Angular, dois apps no mesmo workspace (`landing` com SSR, `admin` SPA)
- Jobs: Hangfire
- PDF: QuestPDF

## Layout

```
src/
  SolarES.Dominio/          # entidades, regras, motor de cálculo — sem dependências
  SolarES.Aplicacao/        # casos de uso, orquestração
  SolarES.Infraestrutura/   # EF Core, HTTP, storage, jobs
  SolarES.Api/              # controllers, DI, autenticação
web/
  landing/                  # Angular SSR, público
  admin/                    # Angular SPA, autenticado
  shared/                   # modelos e clients de API
tests/
  SolarES.Dominio.Tests/    # inclui os testes de regressão do motor
  SolarES.Api.Tests/
docs/                       # planejamento e decisões — ler antes de tarefas grandes
fixtures/                   # contas anonimizadas + resultados esperados
```

## Comandos

```
dotnet build
dotnet test
dotnet test tests/SolarES.Dominio.Tests    # roda a suíte de regressão do motor
dotnet ef migrations add <Nome> -p src/SolarES.Infraestrutura -s src/SolarES.Api
npm run start:landing
npm run start:admin
npm run test:web
```

## Idioma no código

Domínio em português, infraestrutura em inglês. `Proposta`, `Simulacao`,
`CustoDisponibilidade`, `FioB` — nunca traduzir termo de domínio. `Repository`,
`Handler`, `Service`, `Options` seguem em inglês. Sem acento em identificadores.

Essa regra existe porque metade do vocabulário do projeto é regulatório brasileiro e
não tem tradução estável. Misturar os dois idiomas dentro do domínio produz `Proposal`
e `Proposta` na mesma solução.

## Regras que não se negociam

- `SolarES.Dominio` não referencia EF Core, HttpClient, nem nada de I/O. Se uma tarefa
  parecer exigir isso, o desenho está errado — pare e pergunte.
- Toda `Proposta` grava `ConfiguracaoVersaoId` e o snapshot das entradas e do resultado.
  Recalcular uma proposta antiga tem que reproduzir o número original.
- `ConfiguracaoVersao` publicada é imutável. Alteração cria versão nova.
- Nenhum dado vindo de fonte externa altera configuração ativa automaticamente. Sempre
  gera alerta para revisão humana.
- Fixtures em `fixtures/` são anonimizadas. Conta de luz real com nome, CPF, endereço ou
  número de UC nunca entra no repositório.
- Todo parâmetro do cálculo carrega `Origem`: `Lei`, `FontePublica` ou `Provisorio`.
  Enquanto existir premissa `Provisorio` ativa, nenhuma proposta vai a cliente real e o
  PDF sai com marca d'água. Isso é regra de código, coberta por teste.
- Migration já aplicada não se edita. Corrige-se com migration nova.
- Os testes do motor em `tests/SolarES.Dominio.Tests` são o contrato do projeto: a suíte
  de invariantes desde o início, e a de regressão a partir do portão G1. Se um teste
  falhar, o desvio é reportado — nunca ajustado para passar.

## Documentação

- `docs/00-passo-a-passo.md` — sequência literal de execução, com comandos e prompts
- `docs/01-produto-e-escopo.md` — o que é MVP e, principalmente, o que não é
- `docs/02-dominio-e-glossario.md` — vocabulário, regras regulatórias e fórmulas
- `docs/03-arquitetura.md` — camadas e decisões
- `docs/04-modelo-de-dados.md` — entidades e invariantes
- `docs/05-plano-de-execucao.md` — tarefas em ordem, com critério de aceite
- `docs/06-fluxo-com-claude-code.md` — como trabalhar neste repositório

Leia `docs/02` antes de qualquer tarefa que toque no cálculo. O vocabulário de energia
solar e regulação brasileira é específico e o palpite costuma sair errado.

## Convenções

- Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`, `refactor:`)
- Uma branch por tarefa do plano de execução: `feat/T07-motor-financeiro`
- Toda regra de negócio nova entra com teste no mesmo commit
- Valores monetários e tarifas em `decimal`, nunca `double`

## Agent skills

### Issue tracker

Issues live as GitHub issues in `tallisazevedo/sistema-solar` (via `gh`). See
`docs/agents/issue-tracker.md`.

### Triage labels

Os 5 papéis canônicos, sem renomear: `needs-triage`, `needs-info`, `ready-for-agent`,
`ready-for-human`, `wontfix`. Veja `docs/agents/triage-labels.md`.

### Domain docs

Contexto único; o glossário de domínio é `docs/02-dominio-e-glossario.md` (não
`CONTEXT.md`), sem ADRs ainda. Veja `docs/agents/domain.md`.
