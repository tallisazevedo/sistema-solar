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
  origem: number;
  visitaTecnicaAgendadaPara: string | null;
  criadoEm: string;
  roteadoParaHumano: boolean;
  calibracaoPendente: boolean;
  possuiAnexo: boolean;
  consentimentos: { finalidade: number; versaoTexto: string; concedidoEm: string }[];
  simulacaoId: string | null;
  resultado: {
    potenciaInstaladaKwp: number;
    quantidadeModulos: number;
    capex: number;
    economiaMensalAno1: number;
  } | null;
  expurgadoEm: string | null;
}

@Injectable({ providedIn: 'root' })
export class LeadsService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(ApiConfiguration);
  listar(origem?: number, status?: number): Observable<LeadAdmin[]> {
    const parametros = new URLSearchParams();
    if (origem !== undefined) parametros.set('origem', String(origem));
    if (status !== undefined) parametros.set('status', String(status));
    const query = parametros.size ? `?${parametros}` : '';
    return this.http.get<LeadAdmin[]>(`${this.api.rootUrl}/api/leads${query}`);
  }
  obter(id: string): Observable<LeadAdmin> {
    return this.http.get<LeadAdmin>(`${this.api.rootUrl}/api/leads/${id}`);
  }
  baixarAnexo(id: string): Observable<Blob> {
    return this.http.get(`${this.api.rootUrl}/api/leads/${id}/anexo`, { responseType: 'blob' });
  }
  alterarStatus(id: string, status: number, visitaTecnicaAgendadaPara: string | null): Observable<void> {
    return this.http.patch<void>(`${this.api.rootUrl}/api/leads/${id}/status`, { status, visitaTecnicaAgendadaPara });
  }
  criarManual(dados: { nome: string; telefone: string; email: string; origem: number;
    consentimentoContato: boolean; versaoTextoConsentimento: string }): Observable<LeadAdmin> {
    return this.http.post<LeadAdmin>(`${this.api.rootUrl}/api/leads`, dados);
  }
}
