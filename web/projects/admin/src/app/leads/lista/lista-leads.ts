import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LeadAdmin, LeadsService } from '../leads.service';

@Component({
  selector: 'app-lista-leads',
  imports: [DatePipe, RouterLink],
  templateUrl: './lista-leads.html',
  styleUrl: './lista-leads.css',
})
export class ListaLeads implements OnInit {
  private readonly servico = inject(LeadsService);
  protected readonly leads = signal<LeadAdmin[]>([]);
  protected readonly carregando = signal(true);
  ngOnInit(): void {
    this.servico.listar().subscribe({
      next: (leads) => {
        this.leads.set(leads);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false),
    });
  }
}
