export interface MunicipioPublico {
  codigoIbge: string;
  nome: string;
}

export interface CriarSimulacaoPublica {
  consumoMedioMensalKwh: number;
  tipoLigacao: number;
  perfilImovel: number;
  municipioCodigoIbge: string;
  tipoTelhado: number;
  areaDisponivelM2: number;
  possuiGeracaoPropria: boolean;
}

export interface SimulacaoPublica {
  id: string;
  potenciaKwp: number;
  quantidadeModulos: number;
  investimentoEstimado: number;
  economiaMensalAno1: number;
  paybackMeses: number | null;
  calibracaoPendente: boolean;
}

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

  private obter<T>(url: string, init?: RequestInit): Promise<T> {
    return fetch(url, init).then((response) => {
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      return response.json() as Promise<T>;
    });
  }
}
