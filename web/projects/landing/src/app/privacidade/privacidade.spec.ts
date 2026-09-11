import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Privacidade } from './privacidade';

describe('Privacidade', () => {
  it('identifica revisao juridica pendente e finalidade do contato', async () => {
    await TestBed.configureTestingModule({
      imports: [Privacidade],
      providers: [provideRouter([])],
    }).compileComponents();
    const fixture = TestBed.createComponent(Privacidade);
    fixture.detectChanges();
    const texto = fixture.nativeElement.textContent;
    expect(texto).toContain('pendente de revisão jurídica');
    expect(texto).toContain('contato sobre a simulação');
    expect(texto).toContain('contato-comercial-v1');
  });
});
