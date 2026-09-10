import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { Home } from './home/home';
import { Login } from './login/login';

export const routes: Routes = [
  { path: 'login', component: Login },
  { path: '', component: Home, canActivate: [authGuard] },
];
