import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { LeadsService } from '../leads.service';

@Component({ selector: 'app-novo-lead', templateUrl: './novo-lead.html' })
export class NovoLead {
  private readonly servico = inject(LeadsService);
  private readonly router = inject(Router);
  protected readonly erro = signal<string | null>(null);
  protected criar(event: SubmitEvent): void {
    event.preventDefault();
    const dados = new FormData(event.currentTarget as HTMLFormElement);
    this.servico.criarManual({
      nome: String(dados.get('nome')), telefone: String(dados.get('telefone')),
      email: String(dados.get('email')), origem: Number(dados.get('origem')),
      consentimentoContato: dados.has('consentimento'), versaoTextoConsentimento: 'contato-comercial-v1',
    }).subscribe({ next: (lead) => this.router.navigate(['/leads', lead.id]),
      error: (erro) => this.erro.set(erro.error?.detail ?? 'Não foi possível cadastrar o lead.') });
  }
}
