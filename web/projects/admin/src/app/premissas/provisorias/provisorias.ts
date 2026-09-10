import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ConfiguracaoVersaoResponse } from 'shared';
import { AuthService } from '../../core/auth.service';
import { CHAVES_PREMISSA, ChavePremissa, ORIGEM_PROVISORIO, ROTULOS_PREMISSA } from '../rotulos';
import { PremissasService } from '../premissas.service';

interface LinhaProvisoria {
  chave: ChavePremissa;
  rotulo: string;
  valorTexto: string;
  justificativa: string;
}

@Component({
  imports: [],
  selector: 'app-provisorias',
  styleUrl: './provisorias.css',
  templateUrl: './provisorias.html',
})
export class Provisorias implements OnInit {
  private readonly servico = inject(PremissasService);
  private readonly auth = inject(AuthService);

  protected readonly rascunho = signal<ConfiguracaoVersaoResponse | null>(null);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly responsavel = computed(() => this.auth.usuarioAtual()?.nome ?? '');

  protected readonly linhas = computed<LinhaProvisoria[]>(() => {
    const rascunho = this.rascunho();
    if (!rascunho) {
      return [];
    }

    return CHAVES_PREMISSA.filter((chave) => rascunho.payload[chave].origem === ORIGEM_PROVISORIO).map((chave) => ({
      chave,
      rotulo: ROTULOS_PREMISSA[chave],
      valorTexto: this.formatarValor(chave, rascunho.payload[chave].valor),
      justificativa: rascunho.payload[chave].justificativa ?? '',
    }));
  });

  ngOnInit(): void {
    this.servico.obterRascunho().subscribe({
      next: (rascunho) => {
        this.rascunho.set(rascunho);
        this.carregando.set(false);
      },
      error: () => {
        this.erro.set('Nao foi possivel carregar o rascunho.');
        this.carregando.set(false);
      },
    });
  }

  private formatarValor(chave: ChavePremissa, valor: unknown): string {
    if (chave === 'cronogramaFioB') {
      const linhas = (valor as Array<{ ano: number; percentual: number }>) ?? [];
      return linhas.map((l) => `${l.ano}: ${(Number(l.percentual) * 100).toFixed(0)}%`).join(', ');
    }
    if (chave === 'kitLitoral') {
      const kit = valor as { municipiosCodigoIbge: string[]; raioKm: number };
      return `raio ${kit.raioKm} km; ${kit.municipiosCodigoIbge.length} municipio(s) na lista`;
    }
    if (chave === 'custoDisponibilidadePorLigacao') {
      const custo = valor as { monofasica: number; bifasica: number; trifasica: number };
      return `mono ${custo.monofasica} kWh, bi ${custo.bifasica} kWh, tri ${custo.trifasica} kWh`;
    }
    return String(valor);
  }
}
