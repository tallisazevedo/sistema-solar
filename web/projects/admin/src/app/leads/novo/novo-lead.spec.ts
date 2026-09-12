import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideApiConfiguration } from 'shared';
import { NovoLead } from './novo-lead';

describe('NovoLead', () => {
  it('cadastra lead manual com declaracao de consentimento', async () => {
    await TestBed.configureTestingModule({ imports: [NovoLead], providers: [provideRouter([]),
      provideHttpClient(), provideHttpClientTesting(), provideApiConfiguration('')] }).compileComponents();
    const fixture = TestBed.createComponent(NovoLead);
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    fixture.detectChanges();
    const pagina = fixture.nativeElement as HTMLElement;
    pagina.querySelector<HTMLInputElement>('[name="nome"]')!.value = 'Maria';
    pagina.querySelector<HTMLInputElement>('[name="telefone"]')!.value = '27999999999';
    pagina.querySelector<HTMLInputElement>('[name="email"]')!.value = 'maria@teste.com';
    pagina.querySelector<HTMLInputElement>('[name="consentimento"]')!.click();
    pagina.querySelector('form')!.dispatchEvent(new Event('submit'));

    const requisicao = TestBed.inject(HttpTestingController).expectOne('/api/leads');
    expect(requisicao.request.body).toEqual(expect.objectContaining({ nome: 'Maria', origem: 1,
      consentimentoContato: true }));
    requisicao.flush({ id: 'lead-1' });
  });
});
