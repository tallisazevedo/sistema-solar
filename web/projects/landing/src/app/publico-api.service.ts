export interface MunicipioPublico {
  codigoIbge: string;
  nome: string;
}

export interface CriarSimulacaoPublica {
  consumoMedioMensalKwh: number | null;
  historicoConsumoKwh: number[] | null;
  tipoLigacao: number;
  perfilImovel: number;
  municipioCodigoIbge: string;
  tipoTelhado: number;
  areaDisponivelM2: number;
  possuiGeracaoPropria: boolean;
}

export interface SimulacaoPublica {
  id: string;
  potenciaKwp: number | null;
  quantidadeModulos: number | null;
  investimentoEstimado: number | null;
  economiaMensalAno1: number | null;
  paybackMeses: number | null;
  calibracaoPendente: boolean;
  coberturaPercentual: number | null;
  kitLitoral: boolean;
  instalacaoRecomendada: boolean;
  roteadaParaHumano: boolean;
  motivoRoteamento: string | null;
  projecao: { ano: number; anoCalendario: number; economiaLiquidaAnualReais: number }[] | null;
}

export type DesfechoCapturaLead = 'CalibracaoPendente' | 'RoteadoParaHumano' | 'PropostaEmitida';

export class PublicoApiService {
  listarMunicipios(): Promise<MunicipioPublico[]> {
    return this.obter<MunicipioPublico[]>('/api/publico/municipios');
  }

  criarSimulacao(request: CriarSimulacaoPublica): Promise<SimulacaoPublica> {
    return this.obter<SimulacaoPublica>('/api/publico/simulacoes', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    });
  }

  obterSimulacao(id: string): Promise<SimulacaoPublica> {
    return this.obter<SimulacaoPublica>(`/api/publico/simulacoes/${id}`);
  }

  capturarLead(id: string, formulario: FormData): Promise<{ desfecho: DesfechoCapturaLead }> {
    return this.obter(`/api/publico/simulacoes/${id}/lead`, { method: 'POST', body: formulario });
  }

  private obter<T>(url: string, init?: RequestInit): Promise<T> {
    return fetch(url, init).then((response) => {
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      return response.json() as Promise<T>;
    });
  }
}
