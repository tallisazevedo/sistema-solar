import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiConfiguration } from 'shared';

export type StatusProposta = 'Emitida' | 'Vencida' | 'Aceita' | 'Perdida';
export type CanalEnvio = 'Email' | 'Whatsapp';

export type StatusEnvioProposta = 'Pendente' | 'Enviado' | 'Falhou' | 'Entregue';

export interface PropostaAdmin {
  id: string;
  simulacaoId: string;
  numero: string;
  validaAte: string;
  status: StatusProposta;
  enviadaEm: string | null;
  canal: CanalEnvio | null;
  aceitaEm: string | null;
  perdidaEm: string | null;
  motivoPerda: string | null;
  calibracaoPendente: boolean;
}

export interface EnvioPropostaAdmin {
  id: string;
  propostaId: string;
  canal: CanalEnvio;
  destino: string;
  status: StatusEnvioProposta;
  idMensagemProvedor: string | null;
  tentativas: number;
  ultimoErro: string | null;
  enviadoEm: string | null;
  entregueEm: string | null;
  criadoEm: string;
}

@Injectable({ providedIn: 'root' })
export class PropostasService {
  private readonly http = inject(HttpClient);
  private readonly api = inject(ApiConfiguration);

  listar(status?: StatusProposta): Observable<PropostaAdmin[]> {
    const query = status ? `?status=${status}` : '';
    return this.http.get<PropostaAdmin[]>(`${this.api.rootUrl}/api/propostas${query}`);
  }

  obter(id: string): Observable<PropostaAdmin> {
    return this.http.get<PropostaAdmin>(`${this.api.rootUrl}/api/propostas/${id}`);
  }

  aceitar(id: string): Observable<void> {
    return this.http.post<void>(`${this.api.rootUrl}/api/propostas/${id}/aceite`, null);
  }

  marcarPerdida(id: string, motivo: string | null): Observable<void> {
    return this.http.post<void>(`${this.api.rootUrl}/api/propostas/${id}/perda`, { motivo });
  }

  listarEnvios(propostaId: string): Observable<EnvioPropostaAdmin[]> {
    return this.http.get<EnvioPropostaAdmin[]>(`${this.api.rootUrl}/api/propostas/${propostaId}/envios`);
  }

  solicitarEnvio(propostaId: string, canal: CanalEnvio, destino: string): Observable<EnvioPropostaAdmin> {
    return this.http.post<EnvioPropostaAdmin>(`${this.api.rootUrl}/api/propostas/${propostaId}/envios`, { canal, destino });
  }

  reenviar(propostaId: string, envioId: string): Observable<void> {
    return this.http.post<void>(`${this.api.rootUrl}/api/propostas/${propostaId}/envios/${envioId}/reenvio`, null);
  }

  renovar(id: string): Observable<PropostaAdmin> {
    return this.http.post<PropostaAdmin>(`${this.api.rootUrl}/api/propostas/${id}/renovacao`, null);
  }
}
