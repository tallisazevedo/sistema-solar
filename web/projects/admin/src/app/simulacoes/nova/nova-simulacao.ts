import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MunicipioHspResponse, SimulacaoDetalheResponse } from 'shared';
import { OPCOES_SUBGRUPO, OPCOES_TIPO_LIGACAO, OPCOES_TIPO_TELHADO } from '../../catalogo/recursos/enums';
import { SimulacoesService } from '../simulacoes.service';

type ModoConsumo = 'medio' | 'mensal';

@Component({
  imports: [ReactiveFormsModule, DecimalPipe],
  selector: 'app-nova-simulacao',
  styleUrl: './nova-simulacao.css',
  templateUrl: './nova-simulacao.html',
})
export class NovaSimulacao implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly servico = inject(SimulacoesService);

  protected readonly opcoesTipoLigacao = OPCOES_TIPO_LIGACAO;
  protected readonly opcoesSubgrupo = OPCOES_SUBGRUPO;
  protected readonly opcoesTipoTelhado = OPCOES_TIPO_TELHADO;

  protected readonly municipios = signal<MunicipioHspResponse[]>([]);
  protected readonly modo = signal<ModoConsumo>('medio');
  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly resultado = signal<SimulacaoDetalheResponse | null>(null);

  protected readonly formulario = this.fb.nonNullable.group({
    consumoMedio: this.fb.nonNullable.control(500, Validators.required),
    consumoMensal: this.fb.nonNullable.array(
      Array.from({ length: 12 }, () => this.fb.nonNullable.control(500, Validators.required)),
    ),
    tipoLigacao: this.fb.nonNullable.control(0, Validators.required),
    subgrupo: this.fb.nonNullable.control(0, Validators.required),
    municipioCodigoIbge: this.fb.nonNullable.control('', Validators.required),
    tipoTelhado: this.fb.nonNullable.control(0, Validators.required),
    areaDisponivelM2: this.fb.nonNullable.control(50, [Validators.required, Validators.min(0)]),
    possuiGeracaoPropria: this.fb.nonNullable.control(false),
  });

  ngOnInit(): void {
    this.servico.listarMunicipios().subscribe({
      next: (municipios) => {
        this.municipios.set(municipios);
        if (municipios.length > 0) {
          this.formulario.controls.municipioCodigoIbge.setValue(municipios[0].codigoIbge);
        }
      },
    });
  }

  protected get consumoMensal(): FormArray<FormControl<number>> {
    return this.formulario.controls.consumoMensal;
  }

  protected alternarModo(modo: ModoConsumo): void {
    this.modo.set(modo);
  }

  protected simular(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    const v = this.formulario.getRawValue();
    const historicoConsumoKwh = this.modo() === 'medio' ? Array(12).fill(v.consumoMedio) : v.consumoMensal;

    this.enviando.set(true);
    this.erro.set(null);
    this.resultado.set(null);

    this.servico
      .criar({
        historicoConsumoKwh,
        tipoLigacao: v.tipoLigacao,
        subgrupo: v.subgrupo,
        municipioCodigoIbge: v.municipioCodigoIbge,
        tipoTelhado: v.tipoTelhado,
        areaDisponivelM2: v.areaDisponivelM2,
        possuiGeracaoPropria: v.possuiGeracaoPropria,
      })
      .subscribe({
        next: (detalhe) => {
          this.enviando.set(false);
          this.resultado.set(detalhe);
        },
        error: () => {
          this.enviando.set(false);
          this.erro.set('Nao foi possivel gerar a simulacao. Confira os dados e se ha uma versao de configuracao publicada.');
        },
      });
  }
}
