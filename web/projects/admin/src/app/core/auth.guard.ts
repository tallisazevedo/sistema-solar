import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const autenticado = inject(AuthService).autenticado();
  if (autenticado) {
    return true;
  }

  return inject(Router).createUrlTree(['/login']);
};
