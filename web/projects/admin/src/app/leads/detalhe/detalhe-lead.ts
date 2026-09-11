import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LeadAdmin, LeadsService } from '../leads.service';

@Component({
  selector: 'app-detalhe-lead',
  imports: [CurrencyPipe, DecimalPipe, RouterLink],
  templateUrl: './detalhe-lead.html',
  styleUrl: './detalhe-lead.css',
})
export class DetalheLead implements OnInit {
  private readonly rota = inject(ActivatedRoute);
  private readonly servico = inject(LeadsService);
  protected readonly lead = signal<LeadAdmin | null>(null);
  protected readonly erro = signal(false);
  ngOnInit(): void {
    this.servico
      .obter(this.rota.snapshot.paramMap.get('id')!)
      .subscribe({ next: (lead) => this.lead.set(lead), error: () => this.erro.set(true) });
  }
  protected canal(valor: number | null): string {
    return valor === 1 ? 'WhatsApp' : 'E-mail';
  }
}
