import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  {
    path: '',
    renderMode: RenderMode.Prerender,
  },
  { path: 'simular', renderMode: RenderMode.Client },
  { path: 'resultado/:id', renderMode: RenderMode.Client },
  { path: '**', renderMode: RenderMode.Prerender },
];
