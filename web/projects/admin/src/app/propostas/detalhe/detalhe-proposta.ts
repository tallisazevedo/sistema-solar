import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PropostaAdmin, PropostasService } from '../propostas.service';

@Component({
  selector: 'app-detalhe-proposta',
  imports: [DatePipe, RouterLink],
  templateUrl: './detalhe-proposta.html',
  styleUrl: './detalhe-proposta.css',
})
export class DetalheProposta implements OnInit {
  private readonly rota = inject(ActivatedRoute);
  private readonly servico = inject(PropostasService);
  protected readonly proposta = signal<PropostaAdmin | null>(null);
  protected readonly erro = signal(false);
  protected readonly erroAcao = signal<string | null>(null);
  protected readonly motivoPerda = signal('');

  ngOnInit(): void {
    this.carregar();
  }

  protected aceitar(): void {
    const proposta = this.proposta();
    if (!proposta) return;
    this.erroAcao.set(null);
    this.servico.aceitar(proposta.id).subscribe({
      next: () => this.carregar(),
      error: (erro) =>
        this.erroAcao.set(
          erro.status === 409
            ? (erro.error?.detail ?? 'Proposta vencida. Emita uma nova proposta.')
            : 'Não foi possível aceitar a proposta agora.',
        ),
    });
  }

  protected marcarPerdida(): void {
    const proposta = this.proposta();
    if (!proposta) return;
    this.erroAcao.set(null);
    this.servico.marcarPerdida(proposta.id, this.motivoPerda() || null).subscribe({
      next: () => this.carregar(),
      error: () => this.erroAcao.set('Não foi possível marcar a proposta como perdida agora.'),
    });
  }

  private carregar(): void {
    this.servico.obter(this.rota.snapshot.paramMap.get('id')!).subscribe({
      next: (proposta) => this.proposta.set(proposta),
      error: () => this.erro.set(true),
    });
  }
}
