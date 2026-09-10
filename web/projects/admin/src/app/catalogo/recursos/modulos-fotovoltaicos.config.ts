import { HttpClient } from '@angular/common/http';
import {
  ApiConfiguration,
  ModuloFotovoltaicoRequest,
  ModuloFotovoltaicoResponse,
  apiCatalogoModulosFotovoltaicosGet$Json,
  apiCatalogoModulosFotovoltaicosIdDelete,
  apiCatalogoModulosFotovoltaicosIdPut,
  apiCatalogoModulosFotovoltaicosPost$Json,
} from 'shared';
import { RecursoConfig } from '../crud-generico/crud-generico.types';
import { criarApiCrud } from './api-adapter';

export function criarConfigModulosFotovoltaicos(
  http: HttpClient,
  apiConfig: ApiConfiguration,
): RecursoConfig<ModuloFotovoltaicoResponse & { id: string }, ModuloFotovoltaicoRequest> {
  return {
    titulo: 'Modulos fotovoltaicos',
    campos: [
      { chave: 'fabricante', rotulo: 'Fabricante', tipo: 'texto', obrigatorio: true },
      { chave: 'modelo', rotulo: 'Modelo', tipo: 'texto', obrigatorio: true },
      { chave: 'potenciaW', rotulo: 'Potencia (W)', tipo: 'numero', obrigatorio: true },
      { chave: 'larguraMm', rotulo: 'Largura (mm)', tipo: 'numero', obrigatorio: true },
      { chave: 'alturaMm', rotulo: 'Altura (mm)', tipo: 'numero', obrigatorio: true },
      { chave: 'eficienciaPercentual', rotulo: 'Eficiencia (%)', tipo: 'decimal', obrigatorio: true },
      { chave: 'resistenteNevoaSalina', rotulo: 'Resistente a nevoa salina', tipo: 'booleano' },
      { chave: 'ativo', rotulo: 'Ativo', tipo: 'booleano' },
    ],
    api: criarApiCrud(http, apiConfig.rootUrl, {
      listar: apiCatalogoModulosFotovoltaicosGet$Json,
      criar: apiCatalogoModulosFotovoltaicosPost$Json,
      atualizar: apiCatalogoModulosFotovoltaicosIdPut,
      remover: apiCatalogoModulosFotovoltaicosIdDelete,
    }),
    criarRequestVazio: () => ({
      fabricante: '',
      modelo: '',
      potenciaW: 0,
      larguraMm: 0,
      alturaMm: 0,
      eficienciaPercentual: 0,
      resistenteNevoaSalina: false,
      ativo: true,
    }),
    paraRequest: (r) => ({
      fabricante: r.fabricante,
      modelo: r.modelo,
      potenciaW: r.potenciaW,
      larguraMm: r.larguraMm,
      alturaMm: r.alturaMm,
      eficienciaPercentual: r.eficienciaPercentual,
      resistenteNevoaSalina: r.resistenteNevoaSalina,
      ativo: r.ativo,
    }),
  };
}
