import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideApiConfiguration } from 'shared';
import { Funil } from './funil';

describe('Funil', () => {
  it('mostra numerador denominador e percentual do periodo', async () => {
    await TestBed.configureTestingModule({ imports: [Funil], providers: [provideHttpClient(),
      provideHttpClientTesting(), provideApiConfiguration('')] }).compileComponents();
    const fixture = TestBed.createComponent(Funil);
    fixture.detectChanges();
    const pagina = fixture.nativeElement as HTMLElement;
    pagina.querySelector<HTMLInputElement>('[name="de"]')!.value = '2026-09-01';
    pagina.querySelector<HTMLInputElement>('[name="ate"]')!.value = '2026-09-30';
    pagina.querySelector('form')!.dispatchEvent(new Event('submit'));
    const requisicao = TestBed.inject(HttpTestingController).expectOne((req) =>
      req.url.startsWith('/api/metricas/funil?'));
    requisicao.flush({ sessoesIniciadas: 10, sessoesConcluidas: 4, taxaConclusaoPercentual: 40 });
    fixture.detectChanges();
    expect(pagina.textContent).toContain('10');
    expect(pagina.textContent).toContain('4');
    expect(pagina.textContent).toContain('40%');
  });
});
