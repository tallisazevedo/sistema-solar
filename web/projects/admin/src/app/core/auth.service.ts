import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { ApiConfiguration, LoginResponse, apiAuthLoginPost$Json } from 'shared';

const CHAVE_TOKEN = 'solares.admin.token';
const CHAVE_USUARIO = 'solares.admin.usuario';

// Ambiente de teste (vitest) nao expoe localStorage por padrao; em browser real
// sempre existe. Guardamos o acesso pra nao quebrar a suite de testes por causa disso.
const armazenamento = typeof localStorage === 'undefined' ? null : localStorage;

export interface UsuarioAutenticado {
  nome: string;
  perfil: LoginResponse['perfil'];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly usuarioSignal = signal<UsuarioAutenticado | null>(this.lerUsuarioArmazenado());

  readonly usuarioAtual = this.usuarioSignal.asReadonly();
  readonly autenticado = computed(() => this.usuarioSignal() !== null);

  constructor(
    private readonly http: HttpClient,
    private readonly apiConfig: ApiConfiguration,
  ) {}

  login(email: string, senha: string): Observable<LoginResponse> {
    return apiAuthLoginPost$Json(this.http, this.apiConfig.rootUrl, { body: { email, senha } }).pipe(
      map((resposta) => resposta.body),
      tap((corpo) => {
        armazenamento?.setItem(CHAVE_TOKEN, corpo.token);
        armazenamento?.setItem(CHAVE_USUARIO, JSON.stringify({ nome: corpo.nome, perfil: corpo.perfil }));
        this.usuarioSignal.set({ nome: corpo.nome, perfil: corpo.perfil });
      }),
    );
  }

  logout(): void {
    armazenamento?.removeItem(CHAVE_TOKEN);
    armazenamento?.removeItem(CHAVE_USUARIO);
    this.usuarioSignal.set(null);
  }

  obterToken(): string | null {
    return armazenamento?.getItem(CHAVE_TOKEN) ?? null;
  }

  private lerUsuarioArmazenado(): UsuarioAutenticado | null {
    const bruto = armazenamento?.getItem(CHAVE_USUARIO);
    if (!bruto || !armazenamento?.getItem(CHAVE_TOKEN)) {
      return null;
    }

    try {
      return JSON.parse(bruto) as UsuarioAutenticado;
    } catch {
      return null;
    }
  }
}
