import { HttpClient, HttpContext } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { RecursoCrudApi } from '../crud-generico/crud-generico.types';

type FnListar<TResponse> = (
  http: HttpClient,
  rootUrl: string,
  params?: Record<string, never>,
  context?: HttpContext,
) => Observable<{ body: TResponse[] }>;

type FnCriar<TResponse, TRequest> = (
  http: HttpClient,
  rootUrl: string,
  params: { body: TRequest },
  context?: HttpContext,
) => Observable<{ body: TResponse }>;

type FnAtualizar<TRequest> = (
  http: HttpClient,
  rootUrl: string,
  params: { id: string; body: TRequest },
  context?: HttpContext,
) => Observable<unknown>;

type FnRemover = (
  http: HttpClient,
  rootUrl: string,
  params: { id: string },
  context?: HttpContext,
) => Observable<unknown>;

export function criarApiCrud<TResponse, TRequest>(
  http: HttpClient,
  rootUrl: string,
  fns: {
    listar: FnListar<TResponse>;
    criar: FnCriar<TResponse, TRequest>;
    atualizar: FnAtualizar<TRequest>;
    remover: FnRemover;
  },
): RecursoCrudApi<TResponse, TRequest> {
  return {
    listar: () => fns.listar(http, rootUrl).pipe(map((r) => r.body)),
    criar: (body) => fns.criar(http, rootUrl, { body }).pipe(map((r) => r.body)),
    atualizar: (id, body) => fns.atualizar(http, rootUrl, { id, body }).pipe(map(() => undefined)),
    remover: (id) => fns.remover(http, rootUrl, { id }).pipe(map(() => undefined)),
  };
}
