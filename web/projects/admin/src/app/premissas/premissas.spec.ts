import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ConfiguracaoCalculo, ConfiguracaoVersaoResponse } from 'shared';
import { Observable, of, Subject, throwError } from 'rxjs';
import { AuthService } from '../core/auth.service';
import { PremissasService } from './premissas.service';
import { Premissas } from './premissas';

function payload(performanceRatio = 0.78): ConfiguracaoCalculo {
  const premissa = (valor: number) => ({ valor, origem: 2, justificativa: 'Valor provisorio.' });
  return {
    performanceRatio: premissa(performanceRatio),
    degradacaoAnual: premissa(0.005),
    inflacaoTarifaria: premissa(0.05),
    taxaDesconto: premissa(0.1),
    horizonteAnos: premissa(25),
    oversizingMaximo: premissa(1.3),
    fatorOrientacaoPadrao: premissa(0.95),
    limiteKwpRoteamentoHumano: premissa(75),
    estrategiaFioBForaCronograma: premissa(0),
    cronogramaFioB: { valor: [{ ano: 2026, percentual: 0.6 }], origem: 0, justificativa: null },
    kitLitoral: {
      valor: { raioKm: 10, municipiosCodigoIbge: ['3205309'] },
      origem: 2,
      justificativa: 'Valor provisorio.',
    },
    custoDisponibilidadePorLigacao: {
      valor: { monofasica: 30, bifasica: 50, trifasica: 100 },
      origem: 0,
      justificativa: null,
    },
    textosProposta: {
      valor: { disclaimer: 'Valores preliminares.', validadeDias: 15 },
      origem: 2,
      justificativa: 'Valor provisorio.',
    },
  };
}

function versao(configuracao: ConfiguracaoCalculo): ConfiguracaoVersaoResponse {
  return {
    id: '00000000-0000-0000-0000-000000000001',
    numero: 1,
    observacao: null,
    payload: configuracao,
    publicadaEm: null,
    publicadaPorUsuarioId: null,
    status: 0,
  };
}

describe('Premissas', () => {
  let fixture: ComponentFixture<Premissas>;
  let servico: any;

  async function criarCom(
    rascunho: ConfiguracaoVersaoResponse | null,
    ativa: ConfiguracaoVersaoResponse | null,
    rascunho$?: Observable<ConfiguracaoVersaoResponse | null>,
    ativa$?: Observable<ConfiguracaoVersaoResponse | null>,
  ) {
    servico = {
      obterBaseline: vi.fn(() => of(payload())),
      obterRascunho: vi.fn(() => rascunho$ ?? of(rascunho)),
      obterAtiva: vi.fn(() => ativa$ ?? of(ativa)),
      criarRascunho: vi.fn(() => of(versao(payload()))),
      atualizarPayload: vi.fn(() => of(undefined)),
      publicar: vi.fn(() => of(undefined)),
    };
    await TestBed.configureTestingModule({
      imports: [Premissas],
      providers: [
        provideRouter([]),
        { provide: PremissasService, useValue: servico },
        { provide: AuthService, useValue: { usuarioAtual: signal({ nome: 'Dono', perfil: 0 }) } },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(Premissas);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
  }

  it('cria o primeiro rascunho com o baseline da API', async () => {
    await criarCom(null, null);
    botao('Criar primeiro rascunho').click();
    expect(servico.obterBaseline).toHaveBeenCalledOnce();
    expect(servico.criarRascunho).toHaveBeenCalledWith(payload(), null);
  });

  it('copia a versao ativa sem buscar o baseline', async () => {
    const ativa = payload(0.81);
    await criarCom(null, versao(ativa));
    botao('Criar rascunho a partir da versao ativa').click();
    expect(servico.obterBaseline).not.toHaveBeenCalled();
    expect(servico.criarRascunho).toHaveBeenCalledWith(ativa, null);
  });

  it('aguarda a versao ativa antes de liberar a criacao do rascunho', async () => {
    const rascunho$ = new Subject<ConfiguracaoVersaoResponse | null>();
    const ativa$ = new Subject<ConfiguracaoVersaoResponse | null>();
    const configuracaoAtiva = payload(0.81);
    await criarCom(null, null, rascunho$, ativa$);

    rascunho$.next(null);
    rascunho$.complete();
    fixture.detectChanges();
    expect(botaoOpcional('Criar primeiro rascunho')).toBeUndefined();

    ativa$.next(versao(configuracaoAtiva));
    ativa$.complete();
    fixture.detectChanges();
    botao('Criar rascunho a partir da versao ativa').click();
    expect(servico.criarRascunho).toHaveBeenCalledWith(configuracaoAtiva, null);
  });

  it('mostra erro e nao cria rascunho quando o baseline falha', async () => {
    await criarCom(null, null);
    servico.obterBaseline.mockReturnValue(throwError(() => new Error('falha')));
    botao('Criar primeiro rascunho').click();
    fixture.detectChanges();
    expect(servico.criarRascunho).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'baseline',
    );
  });

  it('habilita Publicar quando formulario e rascunho coincidem', async () => {
    await criarCom(versao(payload()), null);
    expect(botao('Publicar').disabled).toBe(false);
  });

  it('desabilita Publicar e explica edicoes nao salvas', async () => {
    await criarCom(versao(payload()), null);
    editarPerformanceRatio(0.8);
    expect(botao('Publicar').disabled).toBe(true);
    expect(fixture.nativeElement.querySelector('.aviso-edicoes-nao-salvas')?.textContent).toContain(
      'Salve o rascunho',
    );
  });

  it('habilita Publicar novamente depois de salvar', async () => {
    await criarCom(versao(payload()), null);
    editarPerformanceRatio(0.8);
    servico.obterRascunho.mockReturnValue(of(versao(payload(0.8))));
    botao('Salvar rascunho').click();
    fixture.detectChanges();
    expect(servico.atualizarPayload).toHaveBeenCalledWith(versao(payload()).id, payload(0.8));
    expect(botao('Publicar').disabled).toBe(false);
  });

  function editarPerformanceRatio(valor: number): void {
    const input = fixture.nativeElement.querySelector(
      'input[formcontrolname="valor"]',
    ) as HTMLInputElement;
    input.value = String(valor);
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
  }

  function botao(texto: string): HTMLButtonElement {
    return botaoOpcional(texto)!;
  }

  function botaoOpcional(texto: string): HTMLButtonElement | undefined {
    return ([...fixture.nativeElement.querySelectorAll('button')] as HTMLButtonElement[]).find(
      (item) => item.textContent?.trim() === texto,
    );
  }
});
