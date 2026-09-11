import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { configurarMetaSemOpenGraph } from '../metadados';
import { MunicipioPublico, PublicoApiService } from '../publico-api.service';

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
    this.api
      .listarMunicipios()
      .then((municipios) => this.municipios.set(municipios))
      .catch(() => this.erro.set('Não foi possível carregar os municípios. Tente novamente.'));
  }

  protected atualizarBusca(event: Event): void {
    this.busca.set((event.target as HTMLInputElement).value);
  }

  protected simular(event: SubmitEvent): void {
    event.preventDefault();
    const dados = new FormData(event.currentTarget as HTMLFormElement);
    this.enviando.set(true);
    this.erro.set(null);
    this.api
      .criarSimulacao({
        consumoMedioMensalKwh: Number(dados.get('consumoMedioMensalKwh')),
        tipoLigacao: Number(dados.get('tipoLigacao')),
        perfilImovel: Number(dados.get('perfilImovel')),
        municipioCodigoIbge: String(dados.get('municipioCodigoIbge')),
        tipoTelhado: Number(dados.get('tipoTelhado')),
        areaDisponivelM2: Number(dados.get('areaDisponivelM2')),
        possuiGeracaoPropria: dados.has('possuiGeracaoPropria'),
      })
      .then(({ id }) => this.router.navigate(['/resultado', id]))
      .catch(() => {
        this.enviando.set(false);
        this.erro.set(
          'Não foi possível calcular agora. Seus dados continuam aqui para tentar novamente.',
        );
      });
  }
}

export default Simulador;
