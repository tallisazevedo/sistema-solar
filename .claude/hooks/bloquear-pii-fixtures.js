#!/usr/bin/env node
// Hook PreToolUse (Write|Edit): bloqueia escrita em fixtures/ quando o conteudo
// parece conter dado pessoal real (CPF, endereco, nome de titular, numero de UC).
// Ver docs/06-fluxo-com-claude-code.md, secao Hooks, e CLAUDE.md.
"use strict";

const fs = require("fs");

function lerEntrada() {
  try {
    return JSON.parse(fs.readFileSync(0, "utf8"));
  } catch {
    return null;
  }
}

function permitir() {
  process.exit(0);
}

function bloquear(motivo) {
  process.stdout.write(
    JSON.stringify({
      hookSpecificOutput: {
        hookEventName: "PreToolUse",
        permissionDecision: "deny",
        permissionDecisionReason: motivo,
      },
    })
  );
  process.exit(0);
}

function cpfValido(digitos) {
  if (!/^\d{11}$/.test(digitos)) return false;
  if (/^(\d)\1{10}$/.test(digitos)) return false;
  const n = digitos.split("").map(Number);
  let soma = 0;
  for (let i = 0; i < 9; i++) soma += n[i] * (10 - i);
  let d1 = (soma * 10) % 11;
  if (d1 === 10) d1 = 0;
  if (d1 !== n[9]) return false;
  soma = 0;
  for (let i = 0; i < 10; i++) soma += n[i] * (11 - i);
  let d2 = (soma * 10) % 11;
  if (d2 === 10) d2 = 0;
  return d2 === n[10];
}

function pareceNomeProprio(valor) {
  // Duas ou mais palavras capitalizadas seguidas: indicio de nome real, nao de
  // placeholder tipo "ANONIMO_001" ou "Cliente Teste 1".
  return /^[A-ZÀ-Ý][a-zà-ÿ]+(?:\s+(?:d[aeo]s?|e)\s+|\s+)[A-ZÀ-Ý][a-zà-ÿ]+(?:\s+[A-ZÀ-Ý][a-zà-ÿ]+)*$/.test(
    valor.trim()
  );
}

const entrada = lerEntrada();
if (!entrada) permitir();

const filePath = (entrada.tool_input && entrada.tool_input.file_path) || "";
if (!/(^|\/)fixtures\//.test(filePath)) permitir();

const conteudo =
  (entrada.tool_input && (entrada.tool_input.content || entrada.tool_input.new_string)) ||
  "";
if (!conteudo) permitir();

const achados = [];

if (/\b\d{3}\.\d{3}\.\d{3}-\d{2}\b/.test(conteudo)) {
  achados.push("CPF formatado (xxx.xxx.xxx-xx)");
}

const candidatosCpf = conteudo.match(/\b\d{11}\b/g) || [];
if (candidatosCpf.some(cpfValido)) {
  achados.push("CPF sem formatacao com digito verificador valido");
}

if (/\b\d{5}-\d{3}\b/.test(conteudo)) {
  achados.push("CEP (indicio de endereco real)");
}

if (/\b(Rua|Av\.|Avenida|Alameda|Travessa|Rod\.|Rodovia)\s+[A-ZÀ-Ý][\wà-ÿ]*(\s+[\wà-ÿ]+)*,?\s*\d+/i.test(conteudo)) {
  achados.push("logradouro com numero (indicio de endereco real)");
}

const campoTitular = /["']?(titular|nomeTitular|nome_titular|nomeCliente|nome_cliente)["']?\s*[:=]\s*["']([^"']+)["']/gi;
let m;
while ((m = campoTitular.exec(conteudo)) !== null) {
  if (pareceNomeProprio(m[2])) {
    achados.push(`campo "${m[1]}" com valor que parece nome proprio real ("${m[2]}")`);
  }
}

const campoUc = /["']?(numeroUc|numero_uc|codigoUc|codigo_uc)["']?\s*[:=]\s*["']?(\d{5,12})["']?/gi;
while ((m = campoUc.exec(conteudo)) !== null) {
  const valor = m[2];
  if (!/^(\d)\1+$/.test(valor) && !/^0+$/.test(valor)) {
    achados.push(`campo "${m[1]}" com numero de UC de aparencia real ("${valor}")`);
  }
}

if (achados.length > 0) {
  bloquear(
    "Bloqueado pelo hook bloquear-pii-fixtures: fixtures/ nao pode receber dado " +
      "pessoal real (regra em CLAUDE.md e docs/06). Padroes encontrados: " +
      achados.join("; ") +
      ". Anonimize antes de gravar."
  );
}

permitir();
