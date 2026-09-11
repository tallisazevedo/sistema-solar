import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { configurarMetaSemOpenGraph } from '../metadados';
import { PublicoApiService, SimulacaoPublica } from '../publico-api.service';

@Component({
  selector: 'app-resultado',
  imports: [RouterLink],
  templateUrl: './resultado.html',
  styleUrl: './resultado.css',
})
export class Resultado implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = new PublicoApiService();
  private readonly meta = inject(Meta);
  private readonly title = inject(Title);
  protected readonly resultado = signal<SimulacaoPublica | null>(null);
  protected readonly erro = signal(false);

  ngOnInit(): void {
    this.title.setTitle('Resultado da simulação | SolarES');
    configurarMetaSemOpenGraph(
      this.meta,
      'Consulte o resultado da sua simulação de energia solar.',
    );
    this.api
      .obterSimulacao(this.route.snapshot.paramMap.get('id')!)
      .then((resultado) => this.resultado.set(resultado))
      .catch(() => this.erro.set(true));
  }

  protected formatarNumero(valor: number, casas = 2): string {
    return new Intl.NumberFormat('pt-BR', {
      minimumFractionDigits: casas,
      maximumFractionDigits: casas,
    }).format(valor);
  }

  protected formatarMoeda(valor: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
  }
}

export default Resultado;
