# 03 — Arquitetura

## Forma geral

Monolito modular em .NET, banco Postgres, dois apps Angular. Nada de microserviço.
O projeto é operado por uma pessoa e o custo de coordenação distribuída não se paga.

## Camadas

```
SolarES.Api            controllers, DI, autenticação, validação de entrada
      ↓
SolarES.Aplicacao      casos de uso, orquestração, transações
      ↓
SolarES.Dominio        entidades, regras, motor de cálculo — zero dependências
      ↑
SolarES.Infraestrutura EF Core, HTTP externo, storage, jobs, PDF
```

`Infraestrutura` implementa interfaces declaradas em `Dominio` e `Aplicacao`. A seta
aponta para cima porque a dependência é invertida.

### A regra central

`SolarES.Dominio` não referencia EF Core, HttpClient, `IConfiguration`, nem nada que
faça I/O. O motor de cálculo recebe um objeto de configuração já materializado e devolve
um resultado. É função pura.

Isso não é purismo. É o que permite rodar o motor contra 50 contas reais em
milissegundos, num teste unitário, sem banco. Essa suíte é o contrato do projeto — sem
ela você não tem como saber se uma refatoração quebrou o cálculo.

## Módulos

Pastas dentro de cada projeto, não projetos separados:

| Módulo | Responsabilidade |
|---|---|
| `Configuracao` | versões de configuração, publicação, resolução da versão ativa |
| `Catalogo` | módulos, inversores, estruturas |
| `Precificacao` | faixas de preço, composição do CAPEX, kit litoral |
| `Simulacao` | motor de cálculo, dimensionamento, financeiro |
| `Conta` | extração da conta de luz, normalização, confirmação |
| `Proposta` | geração, snapshot, PDF, validade |
| `Lead` | captura, origem, status |
| `Identidade` | usuários do admin |

Comunicação entre módulos passa por interface de aplicação, não por acesso direto a
tabela alheia.

## Configuração versionada

O coração do produto. Sequência:

1. Admin edita um rascunho de `ConfiguracaoVersao`
2. Ao publicar, o rascunho vira imutável e recebe número sequencial
3. Toda `Simulacao` e `Proposta` grava o `ConfiguracaoVersaoId` que usou
4. Recalcular uma proposta antiga carrega a versão original e reproduz o número

O payload da versão fica em `jsonb`. Motivo: o formato da configuração vai evoluir muito
nos primeiros meses, e normalizar isso em 12 tabelas cria uma migration por ajuste de
premissa. O admin escreve e lê um objeto tipado, serializado com `System.Text.Json`.

O que **não** entra em `jsonb`: dados que precisam de consulta relacional ou integridade
referencial — catálogo, leads, propostas, usuários. Esses são tabelas normais.

## Extração da conta

**Condicional — Bloco E do doc 05.** Não faz parte do núcleo. A entrada manual é o
caminho de primeira classe e cobre o fluxo inteiro sozinha. O que segue vale quando o
bloco for executado.

Pipeline em três estágios, sempre nesta ordem:

1. **Parser determinístico** da EDP ES, sobre o texto do PDF ou o resultado de OCR
2. **Fallback com modelo de visão**, com schema de saída fixo, quando o parser não bate
3. **Tela de confirmação editável** — obrigatória, sempre, mesmo com confiança alta

O estágio 3 não é opcional e não tem atalho. Ele corrige erro de extração e, de quebra,
aumenta o comprometimento do lead: quem confere os próprios números deixou de ser
curioso.

Processamento assíncrono via Hangfire. Extração e geração de PDF estouram timeout de
request com facilidade.

Chamada a modelo externo é sempre server-side. Chave de API nunca chega ao browser.

## Frontend

Dois apps porque têm requisitos opostos:

**`landing`** — SSR com `@angular/ssr`. Público, anônimo, mobile-first. SEO importa e o
first paint em 4G define a conversão. Mantenha o bundle magro; é a parte do sistema onde
peso custa dinheiro.

**`admin`** — SPA pura, autenticada, sem SEO. Pode ser pesada à vontade.

**`shared`** — biblioteca com modelos e clients de API gerados a partir do OpenAPI.

## Autenticação

- Admin: ASP.NET Identity + JWT
- Landing: anônima
- Portal do cliente (pós-MVP): **sem senha**, por link mágico ou código via WhatsApp,
  com sessão longa. Cliente de energia solar não cria conta. Esse detalhe define se o
  portal é usado ou abandonado.

## Decisões registradas

**Monolito modular, não microserviços.** Uma pessoa desenvolvendo. Complexidade
operacional distribuída não se paga.

**`jsonb` para configuração, tabelas para o resto.** Configuração muda de formato toda
semana no início; catálogo e propostas precisam de integridade e consulta.

**`decimal` para dinheiro e tarifa, nunca `double`.** Erro de arredondamento em tarifa
de R$ 0,074/kWh multiplicado por 25 anos vira divergência visível na proposta.

**`TenantId` em todas as tabelas desde a primeira migration, sem uso.** Único hedge que
vale pagar agora. Adicionar depois é doloroso; deixar reservado é grátis. **Não**
construa isolamento de tenant no MVP.

**QuestPDF para o PDF.** Verificar a licença Community frente ao faturamento da empresa
antes de adotar. Alternativa: PuppeteerSharp renderizando HTML.

**Hangfire para jobs.** Dashboard pronto economiza tempo. Alternativa sem dependência:
tabela de fila com `BackgroundService`.

**Deploy fica para depois do M1.** Decisão consciente. O que não fica para depois é o
`TenantId` e o versionamento de configuração, que são caros de retrofitar.

## O que não fazer

- Não colocar regra de negócio em controller
- Não deixar o motor ler configuração de `appsettings.json` — ela vem do banco, versionada
- Não aplicar dado externo automaticamente na configuração ativa
- Não criar abstração para trocar de banco. É Postgres e vai continuar sendo.
- Não introduzir CQRS, event sourcing ou mediator antes de existir dor concreta
- Não construir o simulador de telhado no MVP, por mais tentador que seja
