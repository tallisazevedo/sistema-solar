import { Routes } from '@angular/router';
import { CrudGenerico } from './catalogo/crud-generico/crud-generico';
import { criarConfigDistribuidoras } from './catalogo/recursos/distribuidoras.config';
import { criarConfigEstruturas } from './catalogo/recursos/estruturas.config';
import { criarConfigFaixasPreco } from './catalogo/recursos/faixas-preco.config';
import { criarConfigInversores } from './catalogo/recursos/inversores.config';
import { criarConfigModulosFotovoltaicos } from './catalogo/recursos/modulos-fotovoltaicos.config';
import { criarConfigMunicipiosHsp } from './catalogo/recursos/municipios-hsp.config';
import { criarConfigTarifasVigentes } from './catalogo/recursos/tarifas-vigentes.config';
import { authGuard } from './core/auth.guard';
import { Home } from './home/home';
import { Login } from './login/login';
import { Premissas } from './premissas/premissas';
import { Provisorias } from './premissas/provisorias/provisorias';
import { Shell } from './shell/shell';
import { ListaSimulacoes } from './simulacoes/lista/lista-simulacoes';
import { NovaSimulacao } from './simulacoes/nova/nova-simulacao';
import { ListaLeads } from './leads/lista/lista-leads';
import { DetalheLead } from './leads/detalhe/detalhe-lead';

export const routes: Routes = [
  { path: 'login', component: Login },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      { path: '', component: Home },
      {
        path: 'catalogo/modulos-fotovoltaicos',
        component: CrudGenerico,
        data: { configFactory: criarConfigModulosFotovoltaicos },
      },
      {
        path: 'catalogo/inversores',
        component: CrudGenerico,
        data: { configFactory: criarConfigInversores },
      },
      {
        path: 'catalogo/estruturas',
        component: CrudGenerico,
        data: { configFactory: criarConfigEstruturas },
      },
      {
        path: 'precos/faixas-preco',
        component: CrudGenerico,
        data: { configFactory: criarConfigFaixasPreco },
      },
      {
        path: 'tarifas/vigentes',
        component: CrudGenerico,
        data: { configFactory: criarConfigTarifasVigentes },
      },
      {
        path: 'tarifas/municipios',
        component: CrudGenerico,
        data: { configFactory: criarConfigMunicipiosHsp },
      },
      {
        path: 'tarifas/distribuidoras',
        component: CrudGenerico,
        data: { configFactory: criarConfigDistribuidoras },
      },
      { path: 'premissas', component: Premissas },
      { path: 'premissas/provisorias', component: Provisorias },
      { path: 'simulacoes', component: ListaSimulacoes },
      { path: 'simulacoes/nova', component: NovaSimulacao },
      { path: 'leads', component: ListaLeads },
      { path: 'leads/:id', component: DetalheLead },
    ],
  },
];
