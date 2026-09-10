import { Component, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

const NOME_PERFIL = ['Dono', 'Vendedor', 'Engenheiro'] as const;

@Component({
  imports: [],
  selector: 'app-home',
  styleUrl: './home.css',
  templateUrl: './home.html',
})
export class Home {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly usuario = this.auth.usuarioAtual;
  protected readonly nomePerfil = computed(() => {
    const usuario = this.usuario();
    return usuario ? NOME_PERFIL[usuario.perfil] : '';
  });

  protected sair(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
