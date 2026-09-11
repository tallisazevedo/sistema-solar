import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiConfiguration } from 'shared';

export interface LeadAdmin {
  id: string;
  nome: string;
  telefone: string;
  email: string;
  canalPreferido: number | null;
  status: number;
  criadoEm: string;
  roteadoParaHumano: boolean;
  calibracaoPendente: boolean;
  possuiAnexo: boolean;
  consentimentos: { finalidade: number; versaoTexto: string; concedidoEm: string }[];
  simulacaoId: string;
  resultado: {
    potenciaInstaladaKwp: number;
    quantidadeModulos: number;
    capex: number;
    economiaMensalAno1: number;
  };
}

@Injectable({ providedIn: 'root' })
export class LeadsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(ApiConfiguration);
  listar(): Observable<LeadAdmin[]> {
    return this.http.get<LeadAdmin[]>(`${this.api.rootUrl}/api/leads`);
  }
  obter(id: string): Observable<LeadAdmin> {
    return this.http.get<LeadAdmin>(`${this.api.rootUrl}/api/leads/${id}`);
  }
  baixarAnexo(id: string): Observable<Blob> {
    return this.http.get(`${this.api.rootUrl}/api/leads/${id}/anexo`, { responseType: 'blob' });
  }
}
