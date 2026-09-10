import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SimulacaoResumoResponse } from 'shared';
import { SimulacoesService } from '../simulacoes.service';

@Component({
  imports: [RouterLink, DecimalPipe, DatePipe],
  selector: 'app-lista-simulacoes',
  styleUrl: './lista-simulacoes.css',
  templateUrl: './lista-simulacoes.html',
})
export class ListaSimulacoes implements OnInit {
  private readonly servico = inject(SimulacoesService);

  protected readonly simulacoes = signal<SimulacaoResumoResponse[]>([]);
  protected readonly carregando = signal(true);

  ngOnInit(): void {
    this.servico.listar().subscribe({
      next: (simulacoes) => {
        this.simulacoes.set(simulacoes);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false),
    });
  }
}
