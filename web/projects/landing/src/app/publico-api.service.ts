export interface MunicipioPublico {
  codigoIbge: string;
  nome: string;
}

export function obterSessaoFunilId(): string {
  const nova = () => {
    if (globalThis.crypto?.randomUUID) return globalThis.crypto.randomUUID();
    const bytes = new Uint8Array(16);
    if (globalThis.crypto?.getRandomValues) globalThis.crypto.getRandomValues(bytes);
    else for (let indice = 0; indice < bytes.length; indice++) bytes[indice] = Math.floor(Math.random() * 256);
    bytes[6] = (bytes[6] & 0x0f) | 0x40;
    bytes[8] = (bytes[8] & 0x3f) | 0x80;
    const hexadecimal = Array.from(bytes, (valor) => valor.toString(16).padStart(2, '0')).join('');
    return `${hexadecimal.slice(0, 8)}-${hexadecimal.slice(8, 12)}-${hexadecimal.slice(12, 16)}-${hexadecimal.slice(16, 20)}-${hexadecimal.slice(20)}`;
  };
  try {
    const chave = 'solares.funil.sessao';
    const existente = sessionStorage.getItem(chave);
    if (existente) return existente;
    const criada = nova();
    sessionStorage.setItem(chave, criada);
    return criada;
  } catch {
    return nova();
  }
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
  sessaoFunilId: string;
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
  registrarInicioSimulacao(sessaoFunilId: string): void {
    const corpo = JSON.stringify({ tipo: 0, sessaoFunilId });
    try {
      if (typeof navigator !== 'undefined' && navigator.sendBeacon &&
          navigator.sendBeacon('/api/publico/eventos', new Blob([corpo], { type: 'application/json' }))) {
        return;
      }
    } catch {
      // Telemetria nunca deve interromper o fluxo principal.
    }
    try {
      void fetch('/api/publico/eventos', { method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: corpo, keepalive: true }).catch(() => undefined);
    } catch {
      // Alguns ambientes podem lançar antes mesmo de devolver uma Promise.
    }
  }
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
