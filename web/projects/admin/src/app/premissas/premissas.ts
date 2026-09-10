import { Component, OnInit, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ConfiguracaoCalculo, ConfiguracaoVersaoResponse } from 'shared';
import { AuthService } from '../core/auth.service';
import { CHAVES_PREMISSA, ChavePremissa, ORIGEM_PREMISSA_ROTULO, ORIGEM_PROVISORIO, ROTULOS_PREMISSA } from './rotulos';
import { PremissasService } from './premissas.service';

function validarJustificativa(grupo: AbstractControl): ValidationErrors | null {
  const origem = grupo.get('origem')?.value;
  const justificativa = ((grupo.get('justificativa')?.value as string) ?? '').trim();
  return origem === ORIGEM_PROVISORIO && !justificativa ? { justificativaObrigatoria: true } : null;
}

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  selector: 'app-premissas',
  styleUrl: './premissas.css',
  templateUrl: './premissas.html',
})
export class Premissas implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly servico = inject(PremissasService);
  private readonly auth = inject(AuthService);

  private readonly chavesEspeciais = ['cronogramaFioB', 'kitLitoral', 'custoDisponibilidadePorLigacao', 'textosProposta'];

  protected readonly rotulos = ROTULOS_PREMISSA;
  protected readonly chavesSimples = CHAVES_PREMISSA.filter((c) => !this.chavesEspeciais.includes(c));
  protected readonly origensRotulo = ORIGEM_PREMISSA_ROTULO;
  protected readonly usuario = this.auth.usuarioAtual;

  protected readonly rascunho = signal<ConfiguracaoVersaoResponse | null>(null);
  protected readonly versaoAtiva = signal<ConfiguracaoVersaoResponse | null>(null);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly mensagem = signal<string | null>(null);
  protected formulario: FormGroup | null = null;

  ngOnInit(): void {
    this.recarregar();
  }

  private recarregar(): void {
    this.carregando.set(true);
    this.erro.set(null);

    this.servico.obterRascunho().subscribe({
      next: (rascunho) => {
        this.rascunho.set(rascunho);
        this.formulario = rascunho ? this.construirFormulario(rascunho.payload) : null;
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set('Nao foi possivel carregar o rascunho.');
        this.carregando.set(false);
      },
    });

    this.servico.obterAtiva().subscribe({ next: (ativa) => this.versaoAtiva.set(ativa) });
  }

  protected criarRascunho(): void {
    const base = this.versaoAtiva()?.payload ?? this.payloadVazio();
    this.servico.criarRascunho(base, null).subscribe({
      next: () => this.recarregar(),
      error: () => this.erro.set('Nao foi possivel criar o rascunho.'),
    });
  }

  protected salvar(): void {
    const rascunho = this.rascunho();
    if (!this.formulario || !rascunho) {
      return;
    }
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      this.erro.set('Existem premissas Provisorias sem justificativa.');
      return;
    }

    this.servico.atualizarPayload(rascunho.id, this.extrairPayload()).subscribe({
      next: () => {
        this.mensagem.set('Rascunho salvo.');
        this.recarregar();
      },
      error: () => this.erro.set('Nao foi possivel salvar o rascunho.'),
    });
  }

  protected publicar(): void {
    const rascunho = this.rascunho();
    if (!rascunho) {
      return;
    }

    this.servico.publicar(rascunho.id).subscribe({
      next: () => {
        this.mensagem.set('Versao publicada.');
        this.recarregar();
      },
      error: () => this.erro.set('Nao foi possivel publicar (confira seu perfil).'),
    });
  }

  protected ehDono(): boolean {
    return this.usuario()?.perfil === 0;
  }

  protected mudou(chave: ChavePremissa): boolean {
    const ativa = this.versaoAtiva();
    const rascunho = this.rascunho();
    if (!ativa || !rascunho) {
      return false;
    }
    return JSON.stringify(ativa.payload[chave]) !== JSON.stringify(rascunho.payload[chave]);
  }

  protected linhasCronograma(): FormArray {
    return this.formulario!.get('cronogramaFioB.linhas') as FormArray;
  }

  protected adicionarLinhaCronograma(): void {
    this.linhasCronograma().push(
      this.fb.group({ ano: this.fb.control(new Date().getFullYear(), Validators.required), percentual: this.fb.control(0, Validators.required) }),
    );
  }

  protected removerLinhaCronograma(indice: number): void {
    this.linhasCronograma().removeAt(indice);
  }

  private construirFormulario(payload: ConfiguracaoCalculo): FormGroup {
    return this.fb.group({
      performanceRatio: this.grupoEscalar(payload.performanceRatio),
      degradacaoAnual: this.grupoEscalar(payload.degradacaoAnual),
      inflacaoTarifaria: this.grupoEscalar(payload.inflacaoTarifaria),
      taxaDesconto: this.grupoEscalar(payload.taxaDesconto),
      horizonteAnos: this.grupoEscalar(payload.horizonteAnos),
      oversizingMaximo: this.grupoEscalar(payload.oversizingMaximo),
      fatorOrientacaoPadrao: this.grupoEscalar(payload.fatorOrientacaoPadrao),
      limiteKwpRoteamentoHumano: this.grupoEscalar(payload.limiteKwpRoteamentoHumano),
      estrategiaFioBForaCronograma: this.grupoEscalar(payload.estrategiaFioBForaCronograma),
      cronogramaFioB: this.fb.group(
        {
          origem: this.fb.control(payload.cronogramaFioB.origem, Validators.required),
          justificativa: this.fb.control(payload.cronogramaFioB.justificativa ?? ''),
          linhas: this.fb.array(
            (payload.cronogramaFioB.valor ?? []).map((linha) =>
              this.fb.group({
                ano: this.fb.control(linha.ano, Validators.required),
                percentual: this.fb.control(linha.percentual, Validators.required),
              }),
            ),
          ),
        },
        { validators: validarJustificativa },
      ),
      kitLitoral: this.fb.group(
        {
          origem: this.fb.control(payload.kitLitoral.origem, Validators.required),
          justificativa: this.fb.control(payload.kitLitoral.justificativa ?? ''),
          raioKm: this.fb.control(payload.kitLitoral.valor.raioKm, Validators.required),
          municipiosTexto: this.fb.control(payload.kitLitoral.valor.municipiosCodigoIbge.join(', ')),
        },
        { validators: validarJustificativa },
      ),
      custoDisponibilidadePorLigacao: this.fb.group(
        {
          origem: this.fb.control(payload.custoDisponibilidadePorLigacao.origem, Validators.required),
          justificativa: this.fb.control(payload.custoDisponibilidadePorLigacao.justificativa ?? ''),
          monofasica: this.fb.control(payload.custoDisponibilidadePorLigacao.valor.monofasica, Validators.required),
          bifasica: this.fb.control(payload.custoDisponibilidadePorLigacao.valor.bifasica, Validators.required),
          trifasica: this.fb.control(payload.custoDisponibilidadePorLigacao.valor.trifasica, Validators.required),
        },
        { validators: validarJustificativa },
      ),
      textosProposta: this.fb.group(
        {
          origem: this.fb.control(payload.textosProposta.origem, Validators.required),
          justificativa: this.fb.control(payload.textosProposta.justificativa ?? ''),
          disclaimer: this.fb.control(payload.textosProposta.valor.disclaimer, Validators.required),
          validadeDias: this.fb.control(payload.textosProposta.valor.validadeDias, Validators.required),
        },
        { validators: validarJustificativa },
      ),
    });
  }

  private grupoEscalar(p: { valor: unknown; origem: number; justificativa?: string | null }): FormGroup {
    return this.fb.group(
      {
        valor: this.fb.control(p.valor, Validators.required),
        origem: this.fb.control(p.origem, Validators.required),
        justificativa: this.fb.control(p.justificativa ?? ''),
      },
      { validators: validarJustificativa },
    );
  }

  private extrairPayload(): ConfiguracaoCalculo {
    const v = this.formulario!.getRawValue();
    const escalar = (chave: string) => ({
      valor: v[chave].valor,
      origem: v[chave].origem,
      justificativa: v[chave].justificativa || null,
    });

    return {
      performanceRatio: escalar('performanceRatio'),
      degradacaoAnual: escalar('degradacaoAnual'),
      inflacaoTarifaria: escalar('inflacaoTarifaria'),
      taxaDesconto: escalar('taxaDesconto'),
      horizonteAnos: escalar('horizonteAnos'),
      oversizingMaximo: escalar('oversizingMaximo'),
      fatorOrientacaoPadrao: escalar('fatorOrientacaoPadrao'),
      limiteKwpRoteamentoHumano: escalar('limiteKwpRoteamentoHumano'),
      estrategiaFioBForaCronograma: escalar('estrategiaFioBForaCronograma'),
      cronogramaFioB: {
        origem: v['cronogramaFioB'].origem,
        justificativa: v['cronogramaFioB'].justificativa || null,
        valor: v['cronogramaFioB'].linhas,
      },
      kitLitoral: {
        origem: v['kitLitoral'].origem,
        justificativa: v['kitLitoral'].justificativa || null,
        valor: {
          raioKm: v['kitLitoral'].raioKm,
          municipiosCodigoIbge: (v['kitLitoral'].municipiosTexto as string)
            .split(',')
            .map((codigo) => codigo.trim())
            .filter(Boolean),
        },
      },
      custoDisponibilidadePorLigacao: {
        origem: v['custoDisponibilidadePorLigacao'].origem,
        justificativa: v['custoDisponibilidadePorLigacao'].justificativa || null,
        valor: {
          monofasica: v['custoDisponibilidadePorLigacao'].monofasica,
          bifasica: v['custoDisponibilidadePorLigacao'].bifasica,
          trifasica: v['custoDisponibilidadePorLigacao'].trifasica,
        },
      },
      textosProposta: {
        origem: v['textosProposta'].origem,
        justificativa: v['textosProposta'].justificativa || null,
        valor: {
          disclaimer: v['textosProposta'].disclaimer,
          validadeDias: v['textosProposta'].validadeDias,
        },
      },
    };
  }

  private payloadVazio(): ConfiguracaoCalculo {
    const escalar = (valor: number) => ({ valor, origem: ORIGEM_PROVISORIO, justificativa: 'Valor inicial a definir.' });
    return {
      performanceRatio: escalar(0),
      degradacaoAnual: escalar(0),
      inflacaoTarifaria: escalar(0),
      taxaDesconto: escalar(0),
      horizonteAnos: escalar(25),
      oversizingMaximo: escalar(1),
      fatorOrientacaoPadrao: escalar(1),
      limiteKwpRoteamentoHumano: escalar(0),
      estrategiaFioBForaCronograma: escalar(0),
      cronogramaFioB: { origem: ORIGEM_PROVISORIO, justificativa: 'Cronograma inicial a definir.', valor: [] },
      kitLitoral: {
        origem: ORIGEM_PROVISORIO,
        justificativa: 'Criterio inicial a definir.',
        valor: { municipiosCodigoIbge: [], raioKm: 0 },
      },
      custoDisponibilidadePorLigacao: {
        origem: ORIGEM_PROVISORIO,
        justificativa: 'Valores iniciais a definir.',
        valor: { monofasica: 30, bifasica: 50, trifasica: 100 },
      },
      textosProposta: {
        origem: ORIGEM_PROVISORIO,
        justificativa: 'Texto inicial a definir.',
        valor: { disclaimer: 'Proposta sujeita a validacao. Numeros preliminares.', validadeDias: 15 },
      },
    };
  }
}
