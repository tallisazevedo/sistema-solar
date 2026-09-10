import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-login',
  styleUrl: './login.css',
  templateUrl: './login.html',
})
export class Login {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly erro = signal(false);
  protected readonly enviando = signal(false);

  protected readonly formulario = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', Validators.required],
  });

  protected entrar(): void {
    if (this.formulario.invalid) {
      return;
    }

    this.erro.set(false);
    this.enviando.set(true);
    const { email, senha } = this.formulario.getRawValue();

    this.auth.login(email, senha).subscribe({
      next: () => {
        this.enviando.set(false);
        this.router.navigateByUrl('/');
      },
      error: () => {
        this.enviando.set(false);
        this.erro.set(true);
      },
    });
  }
}
