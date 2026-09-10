import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideApiConfiguration } from 'shared';
import { Provisorias } from './provisorias';

describe('Provisorias', () => {
  let component: Provisorias;
  let fixture: ComponentFixture<Provisorias>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Provisorias],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), provideApiConfiguration('')],
    }).compileComponents();

    fixture = TestBed.createComponent(Provisorias);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
