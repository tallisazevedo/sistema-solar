---
description: Regras do domínio e do motor de cálculo
globs:
  - "src/SolarES.Dominio/**"
  - "tests/SolarES.Dominio.Tests/**"
---

# Domínio e motor de cálculo

Leia `docs/02-dominio-e-glossario.md` antes de alterar qualquer fórmula.

## Restrições

- Nenhuma dependência de I/O: sem EF Core, sem HttpClient, sem `IConfiguration`,
  sem acesso a arquivo. O motor recebe configuração já materializada.
- `decimal` para dinheiro, tarifa e qualquer valor por kWh. `double` só é aceitável em
  cálculo intermediário de TIR, e o resultado volta para `decimal`.
- Termos de domínio em português, sem acento em identificadores. Não traduza `Proposta`,
  `Simulacao`, `FioB`, `CustoDisponibilidade`.

## Armadilhas que já custaram caro no setor

- O percentual da Lei 14.300 incide sobre o **componente Fio B**, nunca sobre a tarifa
  cheia.
- Custo de disponibilidade não é compensável: 30 kWh monofásico, 50 bifásico,
  100 trifásico. Economia de 100% é impossível e não deve ser produzida por nenhum
  caminho do código.
- Dimensionar sobre média de 12 meses, nunca sobre um mês isolado.
- 2029 em diante não é 100% de Fio B. A ANEEL definirá a metodologia. Use a tabela
  configurada e a regra de fallback; não assuma valor.
- Subgrupo B2 rural tem parâmetros próprios. Não trate como residencial.

## Premissas com origem

Todo parâmetro do cálculo carrega `Origem`: `Lei`, `FontePublica` ou `Provisorio`.

- Premissa `Provisorio` exige justificativa escrita. Nunca crie uma sem.
- Não promova uma premissa de `Provisorio` para validada por conta própria. Só a
  validação com a empresa faz isso, no portão G1.
- Enquanto existir `Provisorio` ativa, a proposta é interna e o PDF sai com marca d'água.
  Esse bloqueio é coberto por teste e não deve ser contornado, nem "temporariamente".

## Testes

A suíte de regressão em `tests/SolarES.Dominio.Tests` roda contra as fixtures reais e é
o contrato do projeto.

Se um teste de regressão falhar, investigue e reporte o desvio. Não relaxe a asserção,
não ajuste o valor esperado, não marque como skip. O gabarito vem de propostas
comerciais reais e é a fonte da verdade.
