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
});
