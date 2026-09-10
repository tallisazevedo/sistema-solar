import { HttpClient } from '@angular/common/http';
import {
  ApiConfiguration,
  FaixaPrecoRequest,
  FaixaPrecoResponse,
  apiPrecificacaoFaixasPrecoGet$Json,
  apiPrecificacaoFaixasPrecoIdDelete,
  apiPrecificacaoFaixasPrecoIdPut,
  apiPrecificacaoFaixasPrecoPost$Json,
} from 'shared';
import { RecursoConfig } from '../crud-generico/crud-generico.types';
import { criarApiCrud } from './api-adapter';

export function criarConfigFaixasPreco(
  http: HttpClient,
  apiConfig: ApiConfiguration,
): RecursoConfig<FaixaPrecoResponse & { id: string }, FaixaPrecoRequest> {
  return {
    titulo: 'Faixas de preco',
    campos: [
      { chave: 'kwpMinimo', rotulo: 'kWp minimo', tipo: 'decimal', obrigatorio: true },
      { chave: 'kwpMaximo', rotulo: 'kWp maximo', tipo: 'decimal', obrigatorio: true },
      { chave: 'precoPorWp', rotulo: 'Preco por Wp (R$)', tipo: 'decimal', obrigatorio: true },
      { chave: 'tipoInstalacao', rotulo: 'Tipo de instalacao', tipo: 'texto', obrigatorio: true },
      { chave: 'kitLitoral', rotulo: 'Kit litoral', tipo: 'booleano' },
      { chave: 'vigencia', rotulo: 'Vigencia', tipo: 'data', obrigatorio: true },
    ],
    api: criarApiCrud(http, apiConfig.rootUrl, {
      listar: apiPrecificacaoFaixasPrecoGet$Json,
      criar: apiPrecificacaoFaixasPrecoPost$Json,
      atualizar: apiPrecificacaoFaixasPrecoIdPut,
      remover: apiPrecificacaoFaixasPrecoIdDelete,
    }),
    criarRequestVazio: () => ({
      kwpMinimo: 0,
      kwpMaximo: 0,
      precoPorWp: 0,
      tipoInstalacao: '',
      kitLitoral: false,
      vigencia: new Date().toISOString().slice(0, 10),
    }),
    paraRequest: (r) => ({
      kwpMinimo: r.kwpMinimo,
      kwpMaximo: r.kwpMaximo,
      precoPorWp: r.precoPorWp,
      tipoInstalacao: r.tipoInstalacao,
      kitLitoral: r.kitLitoral,
      vigencia: r.vigencia,
    }),
  };
}
