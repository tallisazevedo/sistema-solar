import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideApiConfiguration } from 'shared';
import { authInterceptor } from './core/auth.interceptor';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    // rootUrl vazio: em dev, proxy.conf.json encaminha /api pro backend; em producao,
    // o admin e servido do mesmo origin da Api (decisao de deploy, fora do escopo daqui).
    provideApiConfiguration(''),
  ],
};
