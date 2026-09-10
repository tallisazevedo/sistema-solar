import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiConfiguration } from 'shared';
import { CampoConfig, RecursoConfig } from './crud-generico.types';

type ModoFormulario = 'fechado' | 'novo' | 'editar';
type ConfigGenerica = RecursoConfig<{ id: string } & Record<string, unknown>, Record<string, unknown>>;
export type FabricaConfig = (http: HttpClient, apiConfig: ApiConfiguration) => ConfigGenerica | Promise<ConfigGenerica>;

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-crud-generico',
  styleUrl: './crud-generico.css',
  templateUrl: './crud-generico.html',
})
export class CrudGenerico implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);

  protected readonly config = signal<ConfigGenerica | undefined>(undefined);

  protected readonly itens = signal<Array<{ id: string } & Record<string, unknown>>>([]);
  protected readonly carregando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly modo = signal<ModoFormulario>('fechado');
  protected readonly idEditando = signal<string | null>(null);
  protected formulario!: FormGroup;

  async ngOnInit(): Promise<void> {
    const fabrica = this.route.snapshot.data['configFactory'] as FabricaConfig;
    this.config.set(await fabrica(this.http, this.apiConfig));
    this.recarregar();
  }

  protected recarregar(): void {
    this.carregando.set(true);
    this.erro.set(null);
    this.config()!.api.listar().subscribe({
      next: (itens) => {
        this.itens.set(itens);
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set('Nao foi possivel carregar a lista.');
        this.carregando.set(false);
      },
    });
  }

  protected abrirNovo(): void {
    this.idEditando.set(null);
    this.formulario = this.construirFormulario(this.config()!.criarRequestVazio());
    this.modo.set('novo');
  }

  protected abrirEdicao(item: { id: string } & Record<string, unknown>): void {
    this.idEditando.set(item.id);
    this.formulario = this.construirFormulario(this.config()!.paraRequest(item as never));
    this.modo.set('editar');
  }

  protected fechar(): void {
    this.modo.set('fechado');
    this.idEditando.set(null);
  }

  protected salvar(): void {
    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();
      return;
    }

    const corpo = this.normalizarDatas(this.formulario.getRawValue());
    const id = this.idEditando();
    const aoFinalizar = {
      next: () => {
        this.fechar();
        this.recarregar();
      },
      error: () => this.erro.set('Nao foi possivel salvar. Confira os campos.'),
    };

    if (id) {
      this.config()!.api.atualizar(id, corpo).subscribe(aoFinalizar);
    } else {
      this.config()!.api.criar(corpo).subscribe(aoFinalizar);
    }
  }

  protected remover(item: { id: string }): void {
    this.config()!.api.remover(item.id).subscribe({
      next: () => this.recarregar(),
      error: () => this.erro.set('Nao foi possivel remover.'),
    });
  }

  private normalizarDatas(corpo: Record<string, unknown>): Record<string, unknown> {
    const normalizado = { ...corpo };
    for (const campo of this.config()!.campos) {
      if (campo.tipo === 'data' && typeof normalizado[campo.chave] === 'string') {
        const valor = normalizado[campo.chave] as string;
        // <input type="date"> devolve "AAAA-MM-DD" sem offset; DateTimeOffset no backend
        // so aceita offset UTC explicito (bug ja visto na seed da T07).
        normalizado[campo.chave] = valor.length === 10 ? `${valor}T00:00:00Z` : valor;
      }
    }
    return normalizado;
  }

  protected valorExibicao(campo: CampoConfig, item: Record<string, unknown>): unknown {
    const valor = item[campo.chave];
    if (campo.tipo === 'enum') {
      return campo.opcoesEnum?.find((o) => o.valor === valor)?.rotulo ?? valor;
    }
    if (campo.tipo === 'booleano') {
      return valor ? 'Sim' : 'Nao';
    }
    return valor;
  }

  protected campoArray(chave: string): FormArray<FormControl<number>> {
    return this.formulario.get(chave) as FormArray<FormControl<number>>;
  }

  private construirFormulario(valores: Record<string, unknown>): FormGroup {
    const controles: Record<string, FormControl | FormArray> = {};

    for (const campo of this.config()!.campos) {
      controles[campo.chave] = this.construirControle(campo, valores[campo.chave]);
    }

    return new FormGroup(controles);
  }

  private construirControle(campo: CampoConfig, valor: unknown): FormControl | FormArray {
    if (campo.tipo === 'hsp-mensal') {
      const valores = (valor as number[] | undefined) ?? Array(12).fill(0);
      return new FormArray(valores.map((v) => new FormControl(v, { nonNullable: true, validators: [Validators.required] })));
    }

    if (campo.tipo === 'data' && typeof valor === 'string') {
      valor = valor.slice(0, 10);
    }

    const validadores = campo.obrigatorio ? [Validators.required] : [];
    return new FormControl(valor ?? (campo.tipo === 'booleano' ? false : ''), { validators: validadores });
  }
}
