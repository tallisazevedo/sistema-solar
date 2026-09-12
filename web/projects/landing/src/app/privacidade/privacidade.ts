import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-privacidade',
  imports: [RouterLink],
  templateUrl: './privacidade.html',
  styleUrl: './privacidade.css',
})
export class Privacidade {
  constructor() {
    inject(Title).setTitle('Política de privacidade | SolarES');
  }
}

export default Privacidade;
