import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { configurarMetaSemOpenGraph } from '../metadados';
import {
  MunicipioPublico,
  mensagemDeErroPublico,
  obterSessaoFunilId,
  PublicoApiService,
} from '../publico-api.service';

@Component({
  selector: 'app-simulador',
  imports: [RouterLink],
  templateUrl: './simulador.html',
  styleUrl: './simulador.css',
})
export class Simulador implements OnInit {
  private readonly api = new PublicoApiService();
  private readonly router = inject(Router);
  private readonly meta = inject(Meta);
  private readonly title = inject(Title);

  protected readonly municipios = signal<MunicipioPublico[]>([]);
  protected readonly busca = signal('');
  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly etapa = signal(1);
  protected readonly consumoMensal = signal(false);
  protected readonly meses = [
    'Jan',
    'Fev',
    'Mar',
    'Abr',
    'Mai',
    'Jun',
    'Jul',
    'Ago',
    'Set',
    'Out',
    'Nov',
    'Dez',
  ];
  private readonly sessaoFunilId = obterSessaoFunilId();

  protected get municipiosFiltrados(): MunicipioPublico[] {
    const termo = this.busca().trim().toLocaleLowerCase('pt-BR');
    return termo
      ? this.municipios().filter((municipio) =>
          municipio.nome.toLocaleLowerCase('pt-BR').includes(termo),
        )
      : this.municipios();
  }

  ngOnInit(): void {
    this.title.setTitle('Simulador | SolarES');
    configurarMetaSemOpenGraph(
      this.meta,
      'Informe seu consumo para simular sua economia com energia solar.',
    );
    this.api.registrarInicioSimulacao(this.sessaoFunilId);
    this.api
      .listarMunicipios()
      .then((municipios) => this.municipios.set(municipios))
      .catch((erro) => this.erro.set(mensagemDeErroPublico(erro)));
  }

  protected atualizarBusca(event: Event): void {
    this.busca.set((event.target as HTMLInputElement).value);
  }

  protected alternarConsumo(mensal: boolean): void {
    this.consumoMensal.set(mensal);
  }

  protected voltar(): void {
    this.etapa.update((valor) => Math.max(1, valor - 1));
  }

  protected simular(event: SubmitEvent): void {
    event.preventDefault();
    const formulario = event.currentTarget as HTMLFormElement;
    const camposDaEtapa = [
      ...formulario.querySelectorAll<HTMLElement>(
        `fieldset[data-etapa="${this.etapa()}"] input, fieldset[data-etapa="${this.etapa()}"] select`,
      ),
    ];
    if (!camposDaEtapa.every((campo) => (campo as HTMLInputElement).reportValidity())) return;
    if (this.etapa() < 3) {
      this.etapa.update((valor) => valor + 1);
      return;
    }
    const dados = new FormData(formulario);
    const historico = this.consumoMensal()
      ? this.meses.map((_, indice) => Number(dados.get(`consumoMes${indice}`)))
      : null;
    this.enviando.set(true);
    this.erro.set(null);
    this.api
      .criarSimulacao({
        consumoMedioMensalKwh: historico ? null : Number(dados.get('consumoMedioMensalKwh')),
        historicoConsumoKwh: historico,
        tipoLigacao: Number(dados.get('tipoLigacao')),
        perfilImovel: Number(dados.get('perfilImovel')),
        municipioCodigoIbge: String(dados.get('municipioCodigoIbge')),
        tipoTelhado: Number(dados.get('tipoTelhado')),
        areaDisponivelM2: Number(dados.get('largura')) * Number(dados.get('comprimento')),
        possuiGeracaoPropria: dados.has('possuiGeracaoPropria'),
        sessaoFunilId: this.sessaoFunilId,
      })
      .then(({ id }) => this.router.navigate(['/resultado', id]))
      .catch((erro) => {
        this.enviando.set(false);
        this.erro.set(`${mensagemDeErroPublico(erro)} Seus dados continuam aqui para tentar novamente.`);
      });
  }

}

export default Simulador;
