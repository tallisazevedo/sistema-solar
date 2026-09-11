import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { provideApiConfiguration } from 'shared';
import { DetalheLead } from './detalhe-lead';

describe('DetalheLead', () => {
  it('mostra contato, canal e simulacao', async () => {
    await TestBed.configureTestingModule({
      imports: [DetalheLead],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfiguration(''),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: '1' }) } },
        },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(DetalheLead);
    fixture.detectChanges();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/leads/1')
      .flush({
        id: '1',
        nome: 'Maria',
        telefone: '27999999999',
        email: 'maria@exemplo.com',
        canalPreferido: 1,
        roteadoParaHumano: false,
        calibracaoPendente: false,
        possuiAnexo: true,
        consentimentos: [
          {
            finalidade: 0,
            versaoTexto: 'contato-comercial-v1',
            concedidoEm: '2026-09-11T10:00:00Z',
          },
        ],
        resultado: {
          potenciaInstaladaKwp: 5.5,
          quantidadeModulos: 10,
          capex: 20000,
          economiaMensalAno1: 450,
        },
      });
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('maria@exemplo.com');
    expect(texto).toContain('WhatsApp');
    expect(texto).toContain('5.50 kWp');
    expect(texto).toContain('Baixar conta anexada');
    expect(texto).toContain('contato-comercial-v1');
  });
});
