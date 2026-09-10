import { Observable } from 'rxjs';

export type TipoCampo = 'texto' | 'numero' | 'decimal' | 'booleano' | 'enum' | 'data' | 'hsp-mensal';

export interface OpcaoEnum {
  valor: number | string;
  rotulo: string;
}

export interface CampoConfig {
  chave: string;
  rotulo: string;
  tipo: TipoCampo;
  opcoesEnum?: OpcaoEnum[];
  obrigatorio?: boolean;
}

export interface RecursoCrudApi<TResponse, TRequest> {
  listar: () => Observable<TResponse[]>;
  criar: (body: TRequest) => Observable<TResponse>;
  atualizar: (id: string, body: TRequest) => Observable<void>;
  remover: (id: string) => Observable<void>;
}

export interface RecursoConfig<TResponse extends { id: string }, TRequest> {
  titulo: string;
  campos: CampoConfig[];
  api: RecursoCrudApi<TResponse, TRequest>;
  /** Valores default pro formulario de "novo registro". */
  criarRequestVazio: () => TRequest;
  /** Converte uma linha existente pro formato do formulario de edicao. */
  paraRequest: (resposta: TResponse) => TRequest;
}
