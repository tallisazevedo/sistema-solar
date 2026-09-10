import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideApiConfiguration } from 'shared';
import { Premissas } from './premissas';

describe('Premissas', () => {
  let component: Premissas;
  let fixture: ComponentFixture<Premissas>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Premissas],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideApiConfiguration('')],
    }).compileComponents();

    fixture = TestBed.createComponent(Premissas);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
