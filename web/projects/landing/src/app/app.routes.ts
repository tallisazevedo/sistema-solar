import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./home/home').then(({ Home }) => Home),
    title: 'SolarES | Sua economia com energia solar começa aqui',
    data: {
      description:
        'Descubra em minutos quanto você pode economizar com energia solar no Espírito Santo.',
      openGraph: true,
    },
  },
  {
    path: 'simular',
    loadComponent: () =>
      import('./simulador-placeholder/simulador-placeholder').then(
        ({ SimuladorPlaceholder }) => SimuladorPlaceholder,
      ),
    title: 'Simulador | SolarES',
    data: { description: 'Informe seu consumo para simular sua economia com energia solar.' },
  },
  {
    path: 'resultado/:id',
    loadComponent: () =>
      import('./simulador-placeholder/simulador-placeholder').then(
        ({ SimuladorPlaceholder }) => SimuladorPlaceholder,
      ),
    title: 'Resultado da simulação | SolarES',
    data: { description: 'Consulte o resultado da sua simulação de energia solar.' },
  },
  {
    path: '**',
    loadComponent: () =>
      import('./nao-encontrada/nao-encontrada').then(({ NaoEncontrada }) => NaoEncontrada),
    title: 'Página não encontrada | SolarES',
    data: { description: 'O endereço informado não foi encontrado.' },
  },
];
