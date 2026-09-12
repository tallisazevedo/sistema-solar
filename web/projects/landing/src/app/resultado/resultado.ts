import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { configurarMetaSemOpenGraph } from '../metadados';
import {
  DesfechoCapturaLead,
  mensagemDeErroPublico,
  obterSessaoFunilId,
  PublicoApiService,
  SimulacaoPublica,
} from '../publico-api.service';

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
  protected readonly enviando = signal(false);
  protected readonly erroContato = signal<string | null>(null);
  protected readonly desfecho = signal<DesfechoCapturaLead | null>(null);

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

  protected deixarContato(event: SubmitEvent): void {
    event.preventDefault();
    const formulario = event.currentTarget as HTMLFormElement;
    if (!formulario.reportValidity()) return;
    this.enviando.set(true);
    this.erroContato.set(null);
    const dados = new FormData(formulario);
    dados.set('sessaoFunilId', obterSessaoFunilId());
    this.api
      .capturarLead(this.route.snapshot.paramMap.get('id')!, dados)
      .then(({ desfecho }) => this.desfecho.set(desfecho))
      .catch((erro) => {
        this.enviando.set(false);
        this.erroContato.set(`${mensagemDeErroPublico(erro)} Seus dados continuam aqui.`);
      });
  }
}

export default Resultado;
