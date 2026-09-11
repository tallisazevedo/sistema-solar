import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { Resultado } from './resultado';

describe('Resultado', () => {
  it('reabre e apresenta os numeros da simulacao', async () => {
    const fetchMock = vi.fn(() =>
      Promise.resolve({
        ok: true,
        json: () =>
          Promise.resolve({
            id: 'simulacao-1',
            potenciaKwp: 5.5,
            quantidadeModulos: 10,
            investimentoEstimado: 20000,
            economiaMensalAno1: 450,
            paybackMeses: 48,
            calibracaoPendente: true,
          }),
      } as Response),
    );
    vi.stubGlobal('fetch', fetchMock);
    await TestBed.configureTestingModule({
      imports: [Resultado],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: 'simulacao-1' }) } },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(Resultado);
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve, 0));
    await fixture.whenStable();
    fixture.detectChanges();
    const texto = (fixture.nativeElement as HTMLElement).textContent;

    expect(fetchMock).toHaveBeenCalledWith('/api/publico/simulacoes/simulacao-1', undefined);
    expect(texto).toContain('5,50 kWp');
    expect(texto).toContain('10');
    expect(texto).toContain('estimativa preliminar');
  });
});
