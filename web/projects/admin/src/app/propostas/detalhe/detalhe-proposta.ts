import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CanalEnvio, EnvioPropostaAdmin, PropostaAdmin, PropostasService } from '../propostas.service';

@Component({
  selector: 'app-detalhe-proposta',
  imports: [DatePipe, RouterLink],
  templateUrl: './detalhe-proposta.html',
  styleUrl: './detalhe-proposta.css',
})
export class DetalheProposta implements OnInit {
  private readonly rota = inject(ActivatedRoute);
  private readonly servico = inject(PropostasService);
  private readonly router = inject(Router);
  protected readonly proposta = signal<PropostaAdmin | null>(null);
  protected readonly erro = signal(false);
  protected readonly erroAcao = signal<string | null>(null);
  protected readonly motivoPerda = signal('');
  protected readonly envios = signal<EnvioPropostaAdmin[]>([]);
  protected readonly erroEnvio = signal<string | null>(null);
  protected readonly canalEnvio = signal<CanalEnvio>('Email');
  protected readonly destinoEnvio = signal('');

  ngOnInit(): void {
    this.carregar();
    this.carregarEnvios();
  }

  protected solicitarEnvio(): void {
    const proposta = this.proposta();
    if (!proposta || !this.destinoEnvio()) return;
    this.erroEnvio.set(null);
    this.servico.solicitarEnvio(proposta.id, this.canalEnvio(), this.destinoEnvio()).subscribe({
      next: () => {
        this.destinoEnvio.set('');
        this.carregarEnvios();
      },
      error: (erro) =>
        this.erroEnvio.set(
          erro.status === 409
            ? (erro.error?.detail ?? 'Proposta em calibração. Não pode ser enviada ainda.')
            : 'Não foi possível solicitar o envio agora.',
        ),
    });
  }

  protected alterarCanal(valor: string): void {
    this.canalEnvio.set(valor as CanalEnvio);
  }

  protected reenviar(envioId: string): void {
    const proposta = this.proposta();
    if (!proposta) return;
    this.erroEnvio.set(null);
    this.servico.reenviar(proposta.id, envioId).subscribe({
      next: () => this.carregarEnvios(),
      error: () => this.erroEnvio.set('Não foi possível reenviar agora.'),
    });
  }

  private carregarEnvios(): void {
    this.servico
      .listarEnvios(this.rota.snapshot.paramMap.get('id')!)
      .subscribe({ next: (envios) => this.envios.set(envios) });
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

  protected renovar(): void {
    const proposta = this.proposta();
    if (!proposta) return;
    this.erroAcao.set(null);
    this.servico.renovar(proposta.id).subscribe({
      next: (nova) => this.router.navigate(['/propostas', nova.id]),
      error: () => this.erroAcao.set('Não foi possível renovar a proposta agora.'),
    });
  }

  private carregar(): void {
    this.servico.obter(this.rota.snapshot.paramMap.get('id')!).subscribe({
      next: (proposta) => this.proposta.set(proposta),
      error: () => this.erro.set(true),
    });
  }
}
