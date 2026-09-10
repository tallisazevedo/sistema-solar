import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map, retry, timer } from 'rxjs';
import {
  ApiConfiguration,
  EntradaSimulacaoRequest,
  MunicipioHspResponse,
  PropostaResponse,
  SimulacaoDetalheResponse,
  SimulacaoResumoResponse,
  apiSimulacoesGet$Json,
  apiSimulacoesIdGet$Json,
  apiSimulacoesPost$Json,
  apiSimulacoesSimulacaoIdPropostaPost$Json,
  apiTarifasMunicipiosGet$Json,
} from 'shared';

@Injectable({ providedIn: 'root' })
export class SimulacoesService {
  private readonly http = inject(HttpClient);
  private readonly apiConfig = inject(ApiConfiguration);

  criar(entrada: EntradaSimulacaoRequest): Observable<SimulacaoDetalheResponse> {
    return apiSimulacoesPost$Json(this.http, this.apiConfig.rootUrl, { body: entrada }).pipe(map((r) => r.body));
  }

  listar(): Observable<SimulacaoResumoResponse[]> {
    return apiSimulacoesGet$Json(this.http, this.apiConfig.rootUrl).pipe(map((r) => r.body));
  }

  obterPorId(id: string): Observable<SimulacaoDetalheResponse> {
    return apiSimulacoesIdGet$Json(this.http, this.apiConfig.rootUrl, { id }).pipe(map((r) => r.body));
  }

  listarMunicipios(): Observable<MunicipioHspResponse[]> {
    return apiTarifasMunicipiosGet$Json(this.http, this.apiConfig.rootUrl).pipe(map((r) => r.body));
  }

  gerarProposta(simulacaoId: string): Observable<PropostaResponse> {
    return apiSimulacoesSimulacaoIdPropostaPost$Json(this.http, this.apiConfig.rootUrl, { simulacaoId }).pipe(map((r) => r.body));
  }

  // O client gerado nao cobre download binario (a rota nao declara um schema de
  // resposta no OpenAPI); HttpClient direto com responseType: 'blob' e o jeito
  // padrao do Angular de baixar um arquivo autenticado via interceptor.
  baixarPdf(propostaId: string): Observable<Blob> {
    return this.http.get(`${this.apiConfig.rootUrl}/api/propostas/${propostaId}/pdf`, { responseType: 'blob' });
  }

  // T21: geracao do PDF e assincrona (job do Hangfire) -- logo apos o POST, o arquivo
  // pode ainda nao existir (404). Tenta de novo a cada 1s por ate ~15s, que cobre a
  // janela normal (job roda em milissegundos com Postgres local); qualquer outro erro
  // desiste na hora.
  aguardarEGerar(propostaId: string): Observable<Blob> {
    return this.baixarPdf(propostaId).pipe(
      retry({
        count: 15,
        delay: (erro) => {
          if (erro instanceof HttpErrorResponse && erro.status === 404) {
            return timer(1000);
          }
          throw erro;
        },
      }),
    );
  }
}
