import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LeadAdmin, LeadsService } from '../leads.service';

@Component({
  selector: 'app-detalhe-lead',
  imports: [CurrencyPipe, DatePipe, DecimalPipe, RouterLink],
  templateUrl: './detalhe-lead.html',
  styleUrl: './detalhe-lead.css',
})
export class DetalheLead implements OnInit {
  private readonly rota = inject(ActivatedRoute);
  private readonly servico = inject(LeadsService);
  protected readonly lead = signal<LeadAdmin | null>(null);
  protected readonly erro = signal(false);
  protected readonly erroAcao = signal<string | null>(null);
  ngOnInit(): void {
    this.servico
      .obter(this.rota.snapshot.paramMap.get('id')!)
      .subscribe({ next: (lead) => this.lead.set(lead), error: () => this.erro.set(true) });
  }
  protected alterarStatus(status: number, visita: string | null = null): void {
    const lead = this.lead();
    if (!lead) return;
    const visitaComFuso = visita ? new Date(visita).toISOString() : null;
    this.servico.alterarStatus(lead.id, status, visitaComFuso).subscribe({
      next: () => this.ngOnInit(),
      error: (erro) => this.erroAcao.set(erro.error?.detail ?? 'Não foi possível alterar o status.'),
    });
  }
  protected canal(valor: number | null): string {
    return valor === 1 ? 'WhatsApp' : 'E-mail';
  }
  protected baixarAnexo(): void {
    const lead = this.lead();
    if (!lead) return;
    this.servico.baixarAnexo(lead.id).subscribe((arquivo) => {
      const url = URL.createObjectURL(arquivo);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'conta';
      link.click();
      URL.revokeObjectURL(url);
    });
  }

  protected exportarDados(): void {
    const lead = this.lead();
    if (!lead) return;
    this.servico.exportar(lead.id).subscribe((exportacao) => {
      const url = URL.createObjectURL(
        new Blob([JSON.stringify(exportacao, null, 2)], { type: 'application/json' }),
      );
      const link = document.createElement('a');
      link.href = url;
      link.download = `lead-${lead.id}.json`;
      link.click();
      URL.revokeObjectURL(url);
    });
  }

  protected eliminarDados(): void {
    const lead = this.lead();
    if (!lead) return;
    if (!window.confirm('Eliminar os dados pessoais deste lead? Essa ação não pode ser desfeita.')) return;
    this.servico.eliminar(lead.id).subscribe(() => {
      this.servico.obter(lead.id).subscribe((atualizado) => this.lead.set(atualizado));
    });
  }
}
