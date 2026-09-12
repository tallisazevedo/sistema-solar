import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PropostaAdmin, PropostasService, StatusProposta } from '../propostas.service';

@Component({
  selector: 'app-lista-propostas',
  imports: [RouterLink],
  templateUrl: './lista-propostas.html',
  styleUrl: './lista-propostas.css',
})
export class ListaPropostas implements OnInit {
  private readonly servico = inject(PropostasService);
  protected readonly propostas = signal<PropostaAdmin[]>([]);
  protected readonly carregando = signal(true);
  protected readonly filtro = signal<StatusProposta | ''>('');
  protected readonly statusDisponiveis: StatusProposta[] = ['Emitida', 'Vencida', 'Aceita', 'Perdida'];

  ngOnInit(): void {
    this.carregar();
  }

  protected alterarFiltro(status: string): void {
    this.filtro.set(status as StatusProposta | '');
    this.carregar();
  }

  protected diasParaVencer(validaAte: string): number {
    const diferencaMs = new Date(validaAte).getTime() - Date.now();
    return Math.ceil(diferencaMs / (1000 * 60 * 60 * 24));
  }

  private carregar(): void {
    this.carregando.set(true);
    this.servico.listar(this.filtro() || undefined).subscribe({
      next: (propostas) => {
        this.propostas.set(propostas);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false),
    });
  }
}
