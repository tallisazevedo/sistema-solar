import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import {
  ApiConfiguration,
  EntradaSimulacaoRequest,
  MunicipioHspResponse,
  SimulacaoDetalheResponse,
  SimulacaoResumoResponse,
  apiSimulacoesGet$Json,
  apiSimulacoesIdGet$Json,
  apiSimulacoesPost$Json,
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
}
