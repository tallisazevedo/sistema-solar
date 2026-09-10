import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideApiConfiguration } from 'shared';
import { NovaSimulacao } from './nova-simulacao';

describe('NovaSimulacao', () => {
  let component: NovaSimulacao;
  let fixture: ComponentFixture<NovaSimulacao>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NovaSimulacao],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideApiConfiguration('')],
    }).compileComponents();

    fixture = TestBed.createComponent(NovaSimulacao);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
