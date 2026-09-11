import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';

describe('Landing', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes)],
    }).compileComponents();
  });

  it('apresenta a proposta de valor e o caminho para iniciar a simulacao', async () => {
    const fixture = TestBed.createComponent(App);
    await TestBed.inject(Router).navigateByUrl('/');
    fixture.detectChanges();
    await fixture.whenStable();

    const pagina = fixture.nativeElement as HTMLElement;
    expect(pagina.querySelector('h1')?.textContent).toContain(
      'Sua economia com energia solar começa aqui',
    );
    expect(pagina.querySelector<HTMLAnchorElement>('a.cta')?.getAttribute('href')).toBe('/simular');
    expect(pagina.textContent).toContain('Como funciona');
    expect(pagina.textContent).toContain('estimativa preliminar');
    expect(pagina.textContent).toContain('Espírito Santo');
    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toContain(
      'Espírito Santo',
    );
    expect(document.querySelector('meta[property="og:title"]')).not.toBeNull();
  });

  it('mostra uma pagina amigavel para enderecos inexistentes', async () => {
    const fixture = TestBed.createComponent(App);
    await TestBed.inject(Router).navigateByUrl('/endereco-inexistente');
    fixture.detectChanges();
    await fixture.whenStable();

    const pagina = fixture.nativeElement as HTMLElement;
    expect(pagina.querySelector('h1')?.textContent).toContain('Página não encontrada');
    expect(pagina.querySelector<HTMLAnchorElement>('a')?.getAttribute('href')).toBe('/');
    expect(document.querySelector('meta[name="description"]')?.getAttribute('content')).toBe(
      'O endereço informado não foi encontrado.',
    );
    expect(document.querySelector('meta[property="og:title"]')).toBeNull();
  });
});
