import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { Simulador } from './simulador';

describe('Simulador', () => {
  let fixture: ComponentFixture<Simulador>;
  const fetchMock = vi.fn((input: RequestInfo | URL, _init?: RequestInit) =>
    Promise.resolve({
      ok: true,
      json: () =>
        Promise.resolve(
          String(input).endsWith('/municipios')
            ? [{ codigoIbge: '3205309', nome: 'Vitória' }]
            : { id: 'simulacao-1' },
        ),
    } as Response),
  );

  beforeEach(async () => {
    fetchMock.mockClear();
    vi.stubGlobal('fetch', fetchMock);
    await TestBed.configureTestingModule({
      imports: [Simulador],
      providers: [provideRouter([])],
    }).compileComponents();
    fixture = TestBed.createComponent(Simulador);
    fixture.detectChanges();
    await new Promise((resolve) => setTimeout(resolve, 0));
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('envia o consumo medio e abre o resultado', async () => {
    const pagina = fixture.nativeElement as HTMLElement;
    const municipio = pagina.querySelector<HTMLSelectElement>(
      'select[name="municipioCodigoIbge"]',
    )!;
    municipio.selectedIndex = 1;
    municipio.dispatchEvent(new Event('change'));
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);

    const formulario = pagina.querySelector('form')!;
    formulario.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    formulario.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    formulario.dispatchEvent(new Event('submit'));
    await new Promise((resolve) => setTimeout(resolve, 0));
    await fixture.whenStable();

    const [, init] = fetchMock.mock.calls.at(-1)!;
    expect(JSON.parse(init!.body as string)).toEqual(
      expect.objectContaining({ consumoMedioMensalKwh: 500, municipioCodigoIbge: '3205309' }),
    );
    expect(navigate).toHaveBeenCalledWith(['/resultado', 'simulacao-1']);
  });

  it('mantem os dados quando a API falha', async () => {
    const pagina = fixture.nativeElement as HTMLElement;
    const consumo = pagina.querySelector<HTMLInputElement>('[name="consumoMedioMensalKwh"]')!;
    consumo.value = '777';
    const municipio = pagina.querySelector<HTMLSelectElement>('[name="municipioCodigoIbge"]')!;
    municipio.selectedIndex = 1;
    fetchMock.mockRejectedValueOnce(new Error('indisponivel'));
    const formulario = pagina.querySelector('form')!;

    formulario.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    formulario.dispatchEvent(new Event('submit'));
    fixture.detectChanges();
    formulario.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
    fixture.detectChanges();

    expect(consumo.value).toBe('777');
    expect(pagina.textContent).toContain('Seus dados continuam aqui');
  });
});
