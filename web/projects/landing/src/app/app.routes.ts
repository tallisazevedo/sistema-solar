import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./home/home'),
  },
  {
    path: 'simular',
    loadComponent: () => import('./simulador/simulador'),
  },
  {
    path: 'resultado/:id',
    loadComponent: () => import('./resultado/resultado'),
  },
  {
    path: 'privacidade',
    loadComponent: () => import('./privacidade/privacidade'),
  },
  {
    path: '**',
    loadComponent: () => import('./nao-encontrada/nao-encontrada'),
  },
];
