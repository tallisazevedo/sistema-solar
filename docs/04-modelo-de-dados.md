# 04 — Modelo de dados

Postgres. Todas as tabelas têm `Id` (uuid v7 ou identity), `TenantId` (uuid, reservado e
não usado no MVP), `CriadoEm`, `AtualizadoEm`.

## Configuração

### ConfiguracaoVersao

Núcleo do produto.

| Campo | Tipo | Nota |
|---|---|---|
| Numero | int | sequencial, único por tenant |
| Status | enum | `Rascunho`, `Publicada`, `Arquivada` |
| Payload | jsonb | premissas, cronograma Fio B, limites, textos |
| PublicadaEm | timestamptz | nulo enquanto rascunho |
| PublicadaPorUsuarioId | uuid | |
| Observacao | text | por que essa versão existe |

**Invariantes**
- Versão `Publicada` é imutável. Nenhum update no `Payload` depois da publicação.
- Existe no máximo uma versão `Publicada` ativa por vez.
- Existe no máximo um `Rascunho` por vez.

Conteúdo do `Payload` (contrato tipado no domínio):

```
performanceRatio, degradacaoAnual, inflacaoTarifaria, taxaDesconto,
horizonteAnos, oversizingMaximo, fatorOrientacaoPadrao,
cronogramaFioB: [{ ano, percentual }],
limiteKwpRoteamentoHumano,
kitLitoral: { municipios: [...], raioKm, ... },
textosProposta: { disclaimer, validadeDias, ... }
```

O cronograma do Fio B vive aqui, não em código, porque a regra a partir de 2029 ainda
será definida pela ANEEL.

## Catálogo

### ModuloFotovoltaico
`Fabricante`, `Modelo`, `PotenciaW`, `LarguraMm`, `AlturaMm`, `EficienciaPercentual`,
`ResistenteNevoaSalina` (bool), `Ativo`

### Inversor
`Fabricante`, `Modelo`, `PotenciaW`, `QuantidadeMppt`, `Tipo` (`String` | `Micro`),
`Ativo`

### Estrutura
`Descricao`, `TipoTelhado` (`Ceramico`, `Metalico`, `Fibrocimento`, `Laje`, `Solo`),
`ResistenteNevoaSalina`, `Ativo`

## Precificação

### FaixaPreco
`KwpMinimo`, `KwpMaximo`, `PrecoPorWp` (decimal), `TipoInstalacao`, `KitLitoral` (bool),
`Vigencia`

O CAPEX sai da faixa aplicável mais os adicionais configurados. Faixa com `KitLitoral`
verdadeiro só é elegível quando a regra de litoral dispara.

## Tarifas e geografia

### Distribuidora
`Nome`, `SiglaAneel`, `Ativa`

### TarifaVigente
`DistribuidoraId`, `Subgrupo` (`B1`, `B2`, `B3`), `TarifaTe`, `TarifaTusd`,
`ValorFioBPorKwh`, `AliquotaIcms`, `AliquotaPisCofins`, `VigenciaInicio`,
`VigenciaFim`, `ResolucaoHomologatoria`, `Fonte`

`ValorFioBPorKwh` é separado da TUSD de propósito — é sobre ele que o percentual da Lei
14.300 incide.

### MunicipioHsp
`CodigoIbge`, `Nome`, `Latitude`, `Longitude`, `DistanciaMarKm`, `DistribuidoraId`,
`HspPorMes` (jsonb, 12 valores), `Fonte`

Seed único com os 78 municípios capixabas. Dataset fixo, pequeno, editável no admin.
Elimina dependência de API externa em runtime.

## Fluxo comercial

### Lead
`Nome`, `Telefone`, `Email`, `Origem`, `MunicipioId`, `ConsentimentoLgpdEm`, `Status`

### ContaExtraida
`LeadId`, `DistribuidoraId`, `NumeroUc`, `Subgrupo`, `TipoLigacao`
(`Monofasica` | `Bifasica` | `Trifasica`), `HistoricoConsumo` (jsonb, 12 meses),
`TarifaExtraida`, `MetodoExtracao` (`Parser` | `Visao` | `Manual`),
`PayloadBruto` (jsonb), `ConfirmadaPeloClienteEm`, `ImagemDescartadaEm`

**Invariantes**
- Nenhuma `Simulacao` é criada a partir de conta sem `ConfirmadaPeloClienteEm`.
- `ImagemDescartadaEm` preenchido após extração bem-sucedida. A imagem original não
  persiste.
- `PayloadBruto` guarda o que a extração leu, para auditoria — sem nome, CPF ou endereço.

### Simulacao
`LeadId`, `ContaExtraidaId`, `ConfiguracaoVersaoId`, `EntradasSnapshot` (jsonb),
`ResultadoSnapshot` (jsonb), `PotenciaKwp`, `QuantidadeModulos`, `Capex`,
`EconomiaMensalAno1`, `PaybackMeses`, `Tir`, `Vpl`, `CoberturaPercentual`,
`RoteadaParaHumano` (bool), `MotivoRoteamento`

Os campos escalares são desnormalização deliberada, para listar e filtrar sem abrir o
jsonb.

### Proposta
`SimulacaoId`, `Numero`, `ConfiguracaoVersaoId`, `ValidaAte`, `ArquivoPdfUrl`,
`EnviadaEm`, `Canal` (`Email` | `Whatsapp`), `Status`
(`Emitida`, `Vencida`, `Aceita`, `Perdida`)

**Invariantes**
- `ConfiguracaoVersaoId` é obrigatório e copiado da simulação.
- Recalcular uma proposta usa a versão gravada, nunca a ativa.
- `ValidaAte` é obrigatório e calculado a partir de `validadeDias` da configuração.

### Usuario
`Nome`, `Email`, `SenhaHash`, `Perfil` (`Dono` | `Vendedor` | `Engenheiro`), `Ativo`

## Índices

- `ConfiguracaoVersao(Status)` — resolução da versão ativa é a consulta mais quente
- `Lead(CriadoEm desc)`, `Lead(Status)`
- `Simulacao(LeadId)`, `Simulacao(ConfiguracaoVersaoId)`
- `Proposta(Numero)` único, `Proposta(ValidaAte)` para o job de vencimento
- `MunicipioHsp(CodigoIbge)` único
- `TarifaVigente(DistribuidoraId, Subgrupo, VigenciaInicio)`

## Precisão

`decimal(18,6)` para tarifas e valores por kWh. `decimal(18,2)` para valores monetários
apresentados. `double` não aparece em nenhum campo financeiro.

## Retenção e LGPD

- Imagem da conta: descartada após extração bem-sucedida
- `ContaExtraida`: sem nome, CPF ou endereço completo; `NumeroUc` mascarado na exibição
- Lead sem conversão: política de expurgo configurável, padrão 24 meses
- Consentimento registrado com data antes de qualquer upload

## Pós-MVP, já previsto no desenho

`SincronizacaoExterna` (fonte, executadoEm, status, payloadBruto jsonb, hash) e
`AlertaConfiguracao` (tipo, severidade, valorAtual, valorDetectado, fonte,
urlEvidencia, status). Não implementar no MVP, mas o versionamento de configuração já
está desenhado para recebê-los.
