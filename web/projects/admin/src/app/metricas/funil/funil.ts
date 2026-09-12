import { DecimalPipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ApiConfiguration } from 'shared';

interface MetricasFunil { sessoesIniciadas: number; sessoesConcluidas: number; taxaConclusaoPercentual: number; }

@Component({ selector: 'app-funil', imports: [DecimalPipe], templateUrl: './funil.html' })
export class Funil {
  private readonly http = inject(HttpClient);
  private readonly api = inject(ApiConfiguration);
  protected readonly metricas = signal<MetricasFunil | null>(null);
  protected readonly erro = signal(false);

  protected consultar(event: SubmitEvent): void {
    event.preventDefault();
    const dados = new FormData(event.currentTarget as HTMLFormElement);
    const de = new Date(String(dados.get('de'))).toISOString();
    const ate = new Date(`${String(dados.get('ate'))}T23:59:59`).toISOString();
    this.http.get<MetricasFunil>(`${this.api.rootUrl}/api/metricas/funil?de=${encodeURIComponent(de)}&ate=${encodeURIComponent(ate)}`)
      .subscribe({ next: (resultado) => this.metricas.set(resultado), error: () => this.erro.set(true) });
  }
}
