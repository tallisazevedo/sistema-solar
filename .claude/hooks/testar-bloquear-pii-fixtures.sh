#!/usr/bin/env bash
# Teste manual do hook .claude/hooks/bloquear-pii-fixtures.js
#
# Roda o script do hook isoladamente (sem passar pelo Claude Code), simulando o
# JSON que o harness envia via stdin em PreToolUse, e confere se a decisao
# (bloquear/permitir) bate com o esperado.
#
# Uso:
#   bash .claude/hooks/testar-bloquear-pii-fixtures.sh
#
# Rode a partir da raiz do repositorio.

set -euo pipefail

SCRIPT=".claude/hooks/bloquear-pii-fixtures.js"
FALHAS=0
TOTAL=0

# args: nome, json_entrada, "bloquear"|"permitir"
caso() {
  local nome="$1" entrada="$2" esperado="$3" saida
  TOTAL=$((TOTAL + 1))
  saida=$(printf '%s' "$entrada" | node "$SCRIPT")

  if [[ "$esperado" == "bloquear" ]]; then
    if [[ -n "$saida" ]] && grep -q '"permissionDecision":"deny"' <<<"$saida"; then
      echo "OK   - $nome (bloqueado)"
    else
      echo "FALHA - $nome: esperava bloqueio, saida foi: ${saida:-<vazia>}"
      FALHAS=$((FALHAS + 1))
    fi
  else
    if [[ -z "$saida" ]]; then
      echo "OK   - $nome (permitido)"
    else
      echo "FALHA - $nome: esperava permitir, mas bloqueou: $saida"
      FALHAS=$((FALHAS + 1))
    fi
  fi
}

echo "Testando $SCRIPT"
echo "------------------------------------------------------------"

caso "CPF formatado" \
  '{"tool_input":{"file_path":"fixtures/contas/x.json","content":"{\"cpf\":\"123.456.789-09\"}"}}' \
  bloquear

caso "CPF sem formatacao, digito verificador valido" \
  '{"tool_input":{"file_path":"fixtures/contas/x.json","content":"12345678909"}}' \
  bloquear

caso "11 digitos aleatorios sem digito verificador valido" \
  '{"tool_input":{"file_path":"fixtures/contas/x.json","content":"11111111111 99999999999"}}' \
  permitir

caso "CEP (indicio de endereco)" \
  '{"tool_input":{"file_path":"fixtures/contas/x.json","content":"CEP 29050-000"}}' \
  bloquear

caso "Logradouro com numero" \
  '{"tool_input":{"file_path":"fixtures/contas/x.json","content":"Rua das Flores, 123"}}' \
  bloquear

caso "Titular com nome proprio real" \
  '{"tool_input":{"file_path":"fixtures/contas/x.json","content":"{\"titular\": \"Maria Aparecida Souza\"}"}}' \
  bloquear

caso "Titular anonimizado (placeholder)" \
  '{"tool_input":{"file_path":"fixtures/contas/x.json","content":"{\"titular\": \"ANONIMO_001\"}"}}' \
  permitir

caso "numeroUc de aparencia real" \
  '{"tool_input":{"file_path":"fixtures/contas/x.json","content":"{\"numeroUc\": \"7834521\"}"}}' \
  bloquear

caso "numeroUc mascarado com zeros (placeholder)" \
  '{"tool_input":{"file_path":"fixtures/contas/x.json","content":"{\"numeroUc\": \"0000000000\"}"}}' \
  permitir

caso "Edit (new_string) tambem e verificado" \
  '{"tool_input":{"file_path":"fixtures/contas/x.json","old_string":"a","new_string":"cpf: 123.456.789-09"}}' \
  bloquear

caso "Arquivo fora de fixtures/ nunca e verificado" \
  '{"tool_input":{"file_path":"src/SolarES.Dominio/Foo.cs","content":"cpf: 123.456.789-09"}}' \
  permitir

echo "------------------------------------------------------------"
if [[ "$FALHAS" -eq 0 ]]; then
  echo "Todos os $TOTAL casos passaram."
else
  echo "$FALHAS de $TOTAL casos falharam."
  exit 1
fi

cat <<'EOF'

Teste end-to-end (dentro de uma sessao do Claude Code):
  1. Abra uma sessao `claude` na raiz do repositorio.
  2. Peca: "crie fixtures/contas/teste.json com {\"cpf\": \"123.456.789-09\"}"
  3. Confirme que a escrita e recusada e a mensagem do hook aparece.
  4. Peca: "crie fixtures/contas/teste.json com {\"titular\": \"ANONIMO_001\"}"
  5. Confirme que a escrita acontece normalmente.
  6. Apague fixtures/contas/teste.json antes de commitar.
EOF
