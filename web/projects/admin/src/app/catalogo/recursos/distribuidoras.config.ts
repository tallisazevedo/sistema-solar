import { HttpClient } from '@angular/common/http';
import {
  ApiConfiguration,
  DistribuidoraRequest,
  DistribuidoraResponse,
  apiTarifasDistribuidorasGet$Json,
  apiTarifasDistribuidorasIdDelete,
  apiTarifasDistribuidorasIdPut,
  apiTarifasDistribuidorasPost$Json,
} from 'shared';
import { RecursoConfig } from '../crud-generico/crud-generico.types';
import { criarApiCrud } from './api-adapter';

export function criarConfigDistribuidoras(
  http: HttpClient,
  apiConfig: ApiConfiguration,
): RecursoConfig<DistribuidoraResponse & { id: string }, DistribuidoraRequest> {
  return {
    titulo: 'Distribuidoras',
    campos: [
      { chave: 'nome', rotulo: 'Nome', tipo: 'texto', obrigatorio: true },
      { chave: 'siglaAneel', rotulo: 'Sigla ANEEL', tipo: 'texto', obrigatorio: true },
      { chave: 'ativa', rotulo: 'Ativa', tipo: 'booleano' },
    ],
    api: criarApiCrud(http, apiConfig.rootUrl, {
      listar: apiTarifasDistribuidorasGet$Json,
      criar: apiTarifasDistribuidorasPost$Json,
      atualizar: apiTarifasDistribuidorasIdPut,
      remover: apiTarifasDistribuidorasIdDelete,
    }),
    criarRequestVazio: () => ({ nome: '', siglaAneel: '', ativa: true }),
    paraRequest: (r) => ({ nome: r.nome, siglaAneel: r.siglaAneel, ativa: r.ativa }),
  };
}
