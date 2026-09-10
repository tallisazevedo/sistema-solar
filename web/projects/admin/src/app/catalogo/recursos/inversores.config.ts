import { HttpClient } from '@angular/common/http';
import {
  ApiConfiguration,
  InversorRequest,
  InversorResponse,
  apiCatalogoInversoresGet$Json,
  apiCatalogoInversoresIdDelete,
  apiCatalogoInversoresIdPut,
  apiCatalogoInversoresPost$Json,
} from 'shared';
import { RecursoConfig } from '../crud-generico/crud-generico.types';
import { criarApiCrud } from './api-adapter';
import { OPCOES_TIPO_INVERSOR } from './enums';

export function criarConfigInversores(
  http: HttpClient,
  apiConfig: ApiConfiguration,
): RecursoConfig<InversorResponse & { id: string }, InversorRequest> {
  return {
    titulo: 'Inversores',
    campos: [
      { chave: 'fabricante', rotulo: 'Fabricante', tipo: 'texto', obrigatorio: true },
      { chave: 'modelo', rotulo: 'Modelo', tipo: 'texto', obrigatorio: true },
      { chave: 'potenciaW', rotulo: 'Potencia (W)', tipo: 'numero', obrigatorio: true },
      { chave: 'quantidadeMppt', rotulo: 'Quantidade de MPPT', tipo: 'numero', obrigatorio: true },
      { chave: 'tipo', rotulo: 'Tipo', tipo: 'enum', opcoesEnum: OPCOES_TIPO_INVERSOR },
      { chave: 'ativo', rotulo: 'Ativo', tipo: 'booleano' },
    ],
    api: criarApiCrud(http, apiConfig.rootUrl, {
      listar: apiCatalogoInversoresGet$Json,
      criar: apiCatalogoInversoresPost$Json,
      atualizar: apiCatalogoInversoresIdPut,
      remover: apiCatalogoInversoresIdDelete,
    }),
    criarRequestVazio: () => ({
      fabricante: '',
      modelo: '',
      potenciaW: 0,
      quantidadeMppt: 1,
      tipo: 0,
      ativo: true,
    }),
    paraRequest: (r) => ({
      fabricante: r.fabricante,
      modelo: r.modelo,
      potenciaW: r.potenciaW,
      quantidadeMppt: r.quantidadeMppt,
      tipo: r.tipo,
      ativo: r.ativo,
    }),
  };
}
