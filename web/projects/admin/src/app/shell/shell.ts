import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth.service';

const NOME_PERFIL = ['Dono', 'Vendedor', 'Engenheiro'] as const;

@Component({
  imports: [RouterLink, RouterOutlet],
  selector: 'app-shell',
  styleUrl: './shell.css',
  templateUrl: './shell.html',
})
export class Shell {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly usuario = this.auth.usuarioAtual;

  protected nomePerfil(): string {
    const usuario = this.usuario();
    return usuario ? NOME_PERFIL[usuario.perfil] : '';
  }

  protected sair(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
