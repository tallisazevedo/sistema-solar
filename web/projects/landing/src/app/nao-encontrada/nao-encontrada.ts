import { Component } from '@angular/core';
import { Meta } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-nao-encontrada',
  imports: [RouterLink],
  template: `<main>
    <p>Erro 404</p>
    <h1>Página não encontrada</h1>
    <span>O endereço pode ter mudado ou não existe.</span
    ><a routerLink="/">Voltar para a página inicial</a>
  </main>`,
  styles: `
    :host {
      display: grid;
      min-height: 100dvh;
      place-items: center;
      padding: 1.5rem;
      background: var(--cor-fundo);
      color: var(--cor-texto);
      text-align: center;
    }
    main {
      max-width: 35rem;
    }
    p {
      color: var(--cor-destaque);
      font-weight: 800;
      text-transform: uppercase;
      letter-spacing: 0.12em;
    }
    h1 {
      margin: 0.5rem 0 1rem;
      font-size: clamp(2.5rem, 8vw, 5rem);
      line-height: 1;
    }
    span {
      display: block;
      color: var(--cor-texto-secundario);
      font-size: 1.15rem;
    }
    a {
      display: inline-block;
      margin-top: 2rem;
      padding: 1rem 1.25rem;
      border-radius: 0.6rem;
      background: var(--cor-acao);
      color: white;
      font-weight: 800;
      text-decoration: none;
    }
  `,
})
export class NaoEncontrada {
  constructor(meta: Meta) {
    meta.updateTag({ name: 'description', content: 'O endereço informado não foi encontrado.' });
    removerOpenGraph(meta);
  }
}

function removerOpenGraph(meta: Meta): void {
  for (const property of ['og:type', 'og:title', 'og:description', 'og:image']) {
    meta.removeTag(`property="${property}"`);
  }
}
