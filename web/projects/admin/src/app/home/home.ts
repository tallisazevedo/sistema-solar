import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CHAVES_PREMISSA, ORIGEM_PROVISORIO } from '../premissas/rotulos';
import { PremissasService } from '../premissas/premissas.service';

@Component({
  imports: [RouterLink],
  selector: 'app-home',
  styleUrl: './home.css',
  templateUrl: './home.html',
})
export class Home implements OnInit {
  private readonly premissasServico = inject(PremissasService);

  protected readonly quantidadeProvisorias = signal<number | null>(null);

  ngOnInit(): void {
    this.premissasServico.obterRascunho().subscribe({
      next: (rascunho) => {
        if (!rascunho) {
          this.quantidadeProvisorias.set(null);
          return;
        }
        const provisorias = CHAVES_PREMISSA.filter((chave) => rascunho.payload[chave].origem === ORIGEM_PROVISORIO);
        this.quantidadeProvisorias.set(provisorias.length);
      },
      error: () => this.quantidadeProvisorias.set(null),
    });
  }
}
