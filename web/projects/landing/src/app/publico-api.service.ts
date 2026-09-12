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

/**
 * Tratamento central de erro HTTP: toda chamada ao backend passa por `obter`, que
 * classifica a falha (validacao, servidor ou rede) e devolve uma mensagem em portugues
 * pronta para exibir ao visitante -- os componentes nunca leem `response.status` nem
 * fazem parse de ProblemDetails por conta propria.
 */
export type TipoErroPublico = 'validacao' | 'servidor' | 'rede' | 'limite';

const MENSAGEM_ERRO_REDE = 'Não foi possível conectar. Verifique sua internet e tente novamente.';
const MENSAGEM_ERRO_SERVIDOR = 'Ocorreu um erro inesperado. Tente novamente em instantes.';

export class ErroHttpPublico extends Error {
  constructor(
    readonly tipo: TipoErroPublico,
    readonly status: number | null,
    mensagem: string,
  ) {
    super(mensagem);
    this.name = 'ErroHttpPublico';
  }
}

/** Mensagem para exibir ao visitante a partir de qualquer erro capturado num `.catch`. */
export function mensagemDeErroPublico(erro: unknown): string {
  return erro instanceof ErroHttpPublico ? erro.message : MENSAGEM_ERRO_SERVIDOR;
}

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

  private async obter<T>(url: string, init?: RequestInit): Promise<T> {
    let response: Response;
    try {
      response = await fetch(url, init);
    } catch {
      throw new ErroHttpPublico('rede', null, MENSAGEM_ERRO_REDE);
    }

    if (!response.ok) {
      let detalhe: string | undefined;
      try {
        const corpo = (await response.json()) as { detail?: unknown };
        detalhe = typeof corpo?.detail === 'string' ? corpo.detail : undefined;
      } catch {
        // Corpo de erro sem JSON valido (ex.: 413 do servidor web antes da Api) -- segue com mensagem generica.
      }

      if (response.status === 429) {
        const segundos = Number(response.headers.get('Retry-After'));
        const espera = Number.isFinite(segundos) && segundos > 0 ? segundos : 60;
        throw new ErroHttpPublico(
          'limite',
          response.status,
          `Muitas tentativas. Tente novamente em ${espera} segundos.`,
        );
      }

      if (response.status >= 500) {
        throw new ErroHttpPublico('servidor', response.status, MENSAGEM_ERRO_SERVIDOR);
      }
      throw new ErroHttpPublico('validacao', response.status, detalhe ?? MENSAGEM_ERRO_SERVIDOR);
    }

    return response.json() as Promise<T>;
  }
}
