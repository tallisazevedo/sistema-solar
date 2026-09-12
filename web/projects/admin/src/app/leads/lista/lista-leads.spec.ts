import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideApiConfiguration } from 'shared';
import { ListaLeads } from './lista-leads';

describe('ListaLeads', () => {
  it('destaca engenharia e calibracao pendente', async () => {
    await TestBed.configureTestingModule({
      imports: [ListaLeads],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfiguration(''),
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(ListaLeads);
    fixture.detectChanges();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/leads')
      .flush([
        {
          id: '1',
          nome: 'Maria',
          criadoEm: '2026-09-11T10:00:00Z',
          roteadoParaHumano: true,
          calibracaoPendente: true,
        },
      ]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Maria');
    expect(fixture.nativeElement.textContent).toContain('Engenharia');
    expect(fixture.nativeElement.textContent).toContain('Calibração pendente');
  });

  it('envia os filtros de origem e status', async () => {
    await TestBed.configureTestingModule({
      imports: [ListaLeads],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideApiConfiguration('')],
    }).compileComponents();
    const fixture = TestBed.createComponent(ListaLeads);
    fixture.detectChanges();
    const http = TestBed.inject(HttpTestingController);
    http.expectOne('/api/leads').flush([]);
    fixture.detectChanges();
    const seletores = (fixture.nativeElement as HTMLElement).querySelectorAll('select');
    const origem = seletores.item(0) as HTMLSelectElement;
    const status = seletores.item(1) as HTMLSelectElement;
    origem.value = '2';
    origem.dispatchEvent(new Event('change'));
    status.value = '1';
    status.dispatchEvent(new Event('change'));
    (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('button')!.click();
    http.expectOne('/api/leads?origem=2&status=1').flush([]);
  });
});
