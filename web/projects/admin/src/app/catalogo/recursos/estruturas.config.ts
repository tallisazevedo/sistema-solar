import { HttpClient } from '@angular/common/http';
import {
  ApiConfiguration,
  EstruturaRequest,
  EstruturaResponse,
  apiCatalogoEstruturasGet$Json,
  apiCatalogoEstruturasIdDelete,
  apiCatalogoEstruturasIdPut,
  apiCatalogoEstruturasPost$Json,
} from 'shared';
import { RecursoConfig } from '../crud-generico/crud-generico.types';
import { criarApiCrud } from './api-adapter';
import { OPCOES_TIPO_TELHADO } from './enums';

export function criarConfigEstruturas(
  http: HttpClient,
  apiConfig: ApiConfiguration,
): RecursoConfig<EstruturaResponse & { id: string }, EstruturaRequest> {
  return {
    titulo: 'Estruturas',
    campos: [
      { chave: 'descricao', rotulo: 'Descricao', tipo: 'texto', obrigatorio: true },
      { chave: 'tipoTelhado', rotulo: 'Tipo de telhado', tipo: 'enum', opcoesEnum: OPCOES_TIPO_TELHADO },
      { chave: 'resistenteNevoaSalina', rotulo: 'Resistente a nevoa salina', tipo: 'booleano' },
      { chave: 'ativo', rotulo: 'Ativo', tipo: 'booleano' },
    ],
    api: criarApiCrud(http, apiConfig.rootUrl, {
      listar: apiCatalogoEstruturasGet$Json,
      criar: apiCatalogoEstruturasPost$Json,
      atualizar: apiCatalogoEstruturasIdPut,
      remover: apiCatalogoEstruturasIdDelete,
    }),
    criarRequestVazio: () => ({
      descricao: '',
      tipoTelhado: 0,
      resistenteNevoaSalina: false,
      ativo: true,
    }),
    paraRequest: (r) => ({
      descricao: r.descricao,
      tipoTelhado: r.tipoTelhado,
      resistenteNevoaSalina: r.resistenteNevoaSalina,
      ativo: r.ativo,
    }),
  };
}
