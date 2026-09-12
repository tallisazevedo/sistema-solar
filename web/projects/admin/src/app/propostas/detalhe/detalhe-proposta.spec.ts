import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { provideApiConfiguration } from 'shared';
import { DetalheProposta } from './detalhe-proposta';

function propostaBase(overrides: Partial<Record<string, unknown>> = {}) {
  return {
    id: '1',
    simulacaoId: 'sim-1',
    numero: 'PROP-2026-0001',
    validaAte: '2026-12-01T00:00:00Z',
    status: 'Emitida',
    enviadaEm: null,
    canal: null,
    aceitaEm: null,
    perdidaEm: null,
    motivoPerda: null,
    ...overrides,
  };
}

describe('DetalheProposta', () => {
  it('mostra numero, status e acoes disponiveis quando emitida', async () => {
    await TestBed.configureTestingModule({
      imports: [DetalheProposta],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfiguration(''),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: '1' }) } } },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(DetalheProposta);
    fixture.detectChanges();
    TestBed.inject(HttpTestingController).expectOne('/api/propostas/1').flush(propostaBase());
    fixture.detectChanges();

    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('PROP-2026-0001');
    expect(texto).toContain('Registrar aceite');
    expect(texto).toContain('Marcar como perdida');
  });

  it('aceita a proposta e recarrega os dados', async () => {
    await TestBed.configureTestingModule({
      imports: [DetalheProposta],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfiguration(''),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: '1' }) } } },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(DetalheProposta);
    fixture.detectChanges();
    const httpMock = TestBed.inject(HttpTestingController);
    httpMock.expectOne('/api/propostas/1').flush(propostaBase());
    fixture.detectChanges();

    fixture.nativeElement
      .querySelectorAll('button')
      .forEach((botao: HTMLButtonElement) => {
        if (botao.textContent?.includes('Registrar aceite')) botao.click();
      });

    httpMock.expectOne('/api/propostas/1/aceite').flush(null);
    httpMock.expectOne('/api/propostas/1').flush(propostaBase({ status: 'Aceita', aceitaEm: '2026-09-12T00:00:00Z' }));
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Aceita em');
  });

  it('mostra mensagem de conflito ao aceitar proposta vencida', async () => {
    await TestBed.configureTestingModule({
      imports: [DetalheProposta],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfiguration(''),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: '1' }) } } },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(DetalheProposta);
    fixture.detectChanges();
    const httpMock = TestBed.inject(HttpTestingController);
    httpMock.expectOne('/api/propostas/1').flush(propostaBase({ status: 'Vencida' }));
    fixture.detectChanges();

    fixture.nativeElement
      .querySelectorAll('button')
      .forEach((botao: HTMLButtonElement) => {
        if (botao.textContent?.includes('Registrar aceite')) botao.click();
      });

    httpMock
      .expectOne('/api/propostas/1/aceite')
      .flush({ detail: 'Proposta venceu. Emita uma nova proposta.' }, { status: 409, statusText: 'Conflict' });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Proposta venceu');
  });
});
