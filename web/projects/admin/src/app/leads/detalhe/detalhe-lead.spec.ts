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

  it('inicia o atendimento de um lead novo', async () => {
    await TestBed.configureTestingModule({ imports: [DetalheLead], providers: [provideRouter([]),
      provideHttpClient(), provideHttpClientTesting(), provideApiConfiguration(''),
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ id: '1' }) } } }] })
      .compileComponents();
    const fixture = TestBed.createComponent(DetalheLead);
    fixture.detectChanges();
    const http = TestBed.inject(HttpTestingController);
    const lead = { id: '1', nome: 'Maria', telefone: '27', email: 'maria@teste.com', canalPreferido: null,
      status: 0, origem: 1, visitaTecnicaAgendadaPara: null, roteadoParaHumano: false,
      calibracaoPendente: false, possuiAnexo: false, consentimentos: [], simulacaoId: null, resultado: null };
    http.expectOne('/api/leads/1').flush(lead);
    fixture.detectChanges();
    const botao = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('button'))
      .find((item) => item.textContent?.includes('Iniciar atendimento'))!;
    (botao as HTMLButtonElement).click();
    http.expectOne('/api/leads/1/status').flush(null);
    http.expectOne('/api/leads/1').flush({ ...lead, status: 1 });
  });

  it('avisa quando o lead foi expurgado', async () => {
    await TestBed.configureTestingModule({
      imports: [DetalheLead],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideApiConfiguration(''),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: '2' }) } },
        },
      ],
    }).compileComponents();
    const fixture = TestBed.createComponent(DetalheLead);
    fixture.detectChanges();
    TestBed.inject(HttpTestingController)
      .expectOne('/api/leads/2')
      .flush({
        id: '2',
        nome: '[expurgado]',
        telefone: '[expurgado]',
        email: '[expurgado]',
        canalPreferido: 1,
        roteadoParaHumano: false,
        calibracaoPendente: false,
        possuiAnexo: false,
        consentimentos: [],
        resultado: {
          potenciaInstaladaKwp: 5.5,
          quantidadeModulos: 10,
          capex: 20000,
          economiaMensalAno1: 450,
        },
        expurgadoEm: '2026-09-11T10:00:00Z',
      });
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('Lead expurgado em');
  });
});
