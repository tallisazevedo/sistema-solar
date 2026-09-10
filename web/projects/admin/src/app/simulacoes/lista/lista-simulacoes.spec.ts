import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideApiConfiguration } from 'shared';
import { ListaSimulacoes } from './lista-simulacoes';

describe('ListaSimulacoes', () => {
  let component: ListaSimulacoes;
  let fixture: ComponentFixture<ListaSimulacoes>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ListaSimulacoes],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideApiConfiguration('')],
    }).compileComponents();

    fixture = TestBed.createComponent(ListaSimulacoes);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
