import { Component } from '@angular/core';
import { Meta } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-simulador-placeholder',
  imports: [RouterLink],
  template: `<main>
    <a routerLink="/">SolarES</a>
    <h1>Simulador em preparação</h1>
    <p>Em breve você poderá calcular sua estimativa por aqui.</p>
  </main>`,
  styles: `
    :host {
      display: block;
      min-height: 100dvh;
      padding: 2rem;
      background: var(--cor-fundo);
      color: var(--cor-texto);
    }
    main {
      max-width: 45rem;
      margin: 15vh auto;
    }
    a {
      color: var(--cor-destaque);
      font-weight: 800;
    }
    h1 {
      font-size: clamp(2.5rem, 8vw, 5rem);
      line-height: 1;
    }
    p {
      font-size: 1.2rem;
      color: var(--cor-texto-secundario);
    }
  `,
})
export class SimuladorPlaceholder {
  constructor(route: ActivatedRoute, meta: Meta) {
    const resultado = route.snapshot.routeConfig?.path === 'resultado/:id';
    meta.updateTag({
      name: 'description',
      content: resultado
        ? 'Consulte o resultado da sua simulação de energia solar.'
        : 'Informe seu consumo para simular sua economia com energia solar.',
    });
    for (const property of ['og:type', 'og:title', 'og:description', 'og:image']) {
      meta.removeTag(`property="${property}"`);
    }
  }
}
