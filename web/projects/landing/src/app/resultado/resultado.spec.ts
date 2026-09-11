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
            coberturaPercentual: 80,
            kitLitoral: true,
            instalacaoRecomendada: true,
            roteadaParaHumano: false,
            motivoRoteamento: null,
            projecao: [{ ano: 1, anoCalendario: 2027, economiaLiquidaAnualReais: 5000 }],
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
    expect(texto).toContain('80%');
    expect(texto).toContain('kit litoral');
    expect(texto).toContain('Projeção anual');
  });

  it('oculta numeros quando a simulacao exige avaliacao humana', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn(() =>
        Promise.resolve({
          ok: true,
          json: () =>
            Promise.resolve({
              id: 'simulacao-2',
              potenciaKwp: null,
              quantidadeModulos: null,
              investimentoEstimado: null,
              economiaMensalAno1: null,
              paybackMeses: null,
              calibracaoPendente: false,
              coberturaPercentual: null,
              kitLitoral: false,
              instalacaoRecomendada: true,
              roteadaParaHumano: true,
              motivoRoteamento: 'Geração própria informada.',
              projecao: null,
            }),
        } as Response),
      ),
    );
    await TestBed.configureTestingModule({
      imports: [Resultado],
      providers: [
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: 'simulacao-2' }) } },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(Resultado);
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve, 0));
    await fixture.whenStable();
    fixture.detectChanges();
    const texto = (fixture.nativeElement as HTMLElement).textContent!;

    expect(texto).toContain('Vamos avaliar seu projeto');
    expect(texto).toContain('Geração própria informada.');
    expect(texto).not.toContain('Investimento estimado');
    expect(texto).not.toMatch(/R\$|kWp/);
  });
});
