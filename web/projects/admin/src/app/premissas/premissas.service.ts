import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of, throwError } from 'rxjs';
import {
  ApiConfiguration,
  ConfiguracaoCalculo,
  ConfiguracaoVersaoResponse,
  apiConfiguracaoAtivaGet$Json,
  apiConfiguracaoRascunhoGet$Json,
  apiConfiguracaoRascunhosIdPayloadPut,
  apiConfiguracaoRascunhosIdPublicarPost,
  apiConfiguracaoRascunhosPost$Json,
} from 'shared';

@Injectable({ providedIn: 'root' })
export class PremissasService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);

  obterRascunho(): Observable<ConfiguracaoVersaoResponse | null> {
    return apiConfiguracaoRascunhoGet$Json(this.http, this.apiConfig.rootUrl).pipe(
      map((r) => r.body),
      catchError((erro) => this.tratarNotFoundComoNulo(erro)),
    );
  }

  obterAtiva(): Observable<ConfiguracaoVersaoResponse | null> {
    return apiConfiguracaoAtivaGet$Json(this.http, this.apiConfig.rootUrl).pipe(
      map((r) => r.body),
      catchError((erro) => this.tratarNotFoundComoNulo(erro)),
    );
  }

  criarRascunho(payload: ConfiguracaoCalculo, observacao: string | null): Observable<ConfiguracaoVersaoResponse> {
    return apiConfiguracaoRascunhosPost$Json(this.http, this.apiConfig.rootUrl, {
      body: { payload, observacao },
    }).pipe(map((r) => r.body));
  }

  atualizarPayload(id: string, payload: ConfiguracaoCalculo): Observable<void> {
    return apiConfiguracaoRascunhosIdPayloadPut(this.http, this.apiConfig.rootUrl, {
      id,
      body: { payload },
    }).pipe(map(() => undefined));
  }

  publicar(id: string): Observable<void> {
    return apiConfiguracaoRascunhosIdPublicarPost(this.http, this.apiConfig.rootUrl, { id }).pipe(map(() => undefined));
  }

  private tratarNotFoundComoNulo(erro: unknown): Observable<null> {
    if (erro instanceof HttpErrorResponse && erro.status === 404) {
      return of(null);
    }
    return throwError(() => erro);
  }
}
