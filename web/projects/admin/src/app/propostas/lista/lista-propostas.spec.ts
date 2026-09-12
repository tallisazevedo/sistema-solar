import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideApiConfiguration } from 'shared';
import { ListaPropostas } from './lista-propostas';

describe('ListaPropostas', () => {
  it('lista propostas e mostra dias para vencer', async () => {
    await TestBed.configureTestingModule({
      imports: [ListaPropostas],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideApiConfiguration('')],
    }).compileComponents();
    const fixture = TestBed.createComponent(ListaPropostas);
    fixture.detectChanges();
    const validaAte = new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/propostas')
      .flush([
        {
          id: '1',
          simulacaoId: 'sim-1',
          numero: 'PROP-2026-0001',
          validaAte,
          status: 'Emitida',
          enviadaEm: null,
          canal: null,
          aceitaEm: null,
          perdidaEm: null,
          motivoPerda: null,
        },
      ]);
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('PROP-2026-0001');
    expect(texto).toContain('3 dias para vencer');
  });

  it('reconsulta a api ao trocar o filtro de status', async () => {
    await TestBed.configureTestingModule({
      imports: [ListaPropostas],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideApiConfiguration('')],
    }).compileComponents();
    const fixture = TestBed.createComponent(ListaPropostas);
    fixture.detectChanges();
    const httpMock = TestBed.inject(HttpTestingController);
    httpMock.expectOne('/api/propostas').flush([]);
    fixture.detectChanges();

    const select = fixture.nativeElement.querySelector('select') as HTMLSelectElement;
    select.value = 'Aceita';
    select.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    httpMock.expectOne('/api/propostas?status=Aceita').flush([]);
  });
});
