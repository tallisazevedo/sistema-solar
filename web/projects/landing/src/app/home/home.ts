import { Component } from '@angular/core';
import { Meta } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  constructor(meta: Meta) {
    meta.updateTag({
      name: 'description',
      content:
        'Descubra em minutos quanto você pode economizar com energia solar no Espírito Santo.',
    });
    meta.updateTag({ property: 'og:type', content: 'website' });
    meta.updateTag({ property: 'og:title', content: 'SolarES | Sua economia começa aqui' });
    meta.updateTag({
      property: 'og:description',
      content: 'Faça uma estimativa gratuita de energia solar para o seu imóvel.',
    });
    meta.updateTag({ property: 'og:image', content: '/solar-es-compartilhamento.svg' });
  }
}
