import { HttpClient } from '@angular/common/http';
import { firstValueFrom, map } from 'rxjs';
import {
  ApiConfiguration,
  TarifaVigenteRequest,
  TarifaVigenteResponse,
  apiTarifasDistribuidorasGet$Json,
  apiTarifasVigentesGet$Json,
  apiTarifasVigentesIdDelete,
  apiTarifasVigentesIdPut,
  apiTarifasVigentesPost$Json,
} from 'shared';
import { RecursoConfig } from '../crud-generico/crud-generico.types';
import { criarApiCrud } from './api-adapter';
import { OPCOES_SUBGRUPO } from './enums';

export async function criarConfigTarifasVigentes(
  http: HttpClient,
  apiConfig: ApiConfiguration,
): Promise<RecursoConfig<TarifaVigenteResponse & { id: string }, TarifaVigenteRequest>> {
  const distribuidoras = await firstValueFrom(
    apiTarifasDistribuidorasGet$Json(http, apiConfig.rootUrl).pipe(map((r) => r.body)),
  );
  const opcoesDistribuidora = distribuidoras.map((d) => ({ valor: d.id, rotulo: `${d.nome} (${d.siglaAneel})` }));

  return {
    titulo: 'Tarifas vigentes',
    campos: [
      { chave: 'distribuidoraId', rotulo: 'Distribuidora', tipo: 'enum', opcoesEnum: opcoesDistribuidora, obrigatorio: true },
      { chave: 'subgrupo', rotulo: 'Subgrupo', tipo: 'enum', opcoesEnum: OPCOES_SUBGRUPO },
      { chave: 'tarifaTe', rotulo: 'Tarifa TE (R$/kWh)', tipo: 'decimal', obrigatorio: true },
      { chave: 'tarifaTusd', rotulo: 'Tarifa TUSD (R$/kWh)', tipo: 'decimal', obrigatorio: true },
      { chave: 'valorFioBPorKwh', rotulo: 'Valor Fio B (R$/kWh)', tipo: 'decimal', obrigatorio: true },
      { chave: 'aliquotaIcms', rotulo: 'Aliquota ICMS (0-1)', tipo: 'decimal', obrigatorio: true },
      { chave: 'aliquotaPisCofins', rotulo: 'Aliquota PIS/COFINS (0-1)', tipo: 'decimal', obrigatorio: true },
      { chave: 'vigenciaInicio', rotulo: 'Vigencia inicio', tipo: 'data', obrigatorio: true },
      { chave: 'resolucaoHomologatoria', rotulo: 'Resolucao homologatoria', tipo: 'texto', obrigatorio: true },
      { chave: 'fonte', rotulo: 'Fonte', tipo: 'texto', obrigatorio: true },
    ],
    api: criarApiCrud(http, apiConfig.rootUrl, {
      listar: apiTarifasVigentesGet$Json,
      criar: apiTarifasVigentesPost$Json,
      atualizar: apiTarifasVigentesIdPut,
      remover: apiTarifasVigentesIdDelete,
    }),
    criarRequestVazio: () => ({
      distribuidoraId: opcoesDistribuidora[0]?.valor as string,
      subgrupo: 0,
      tarifaTe: 0,
      tarifaTusd: 0,
      valorFioBPorKwh: 0,
      aliquotaIcms: 0,
      aliquotaPisCofins: 0,
      vigenciaInicio: new Date().toISOString().slice(0, 10),
      vigenciaFim: null,
      resolucaoHomologatoria: '',
      fonte: '',
    }),
    paraRequest: (r) => ({
      distribuidoraId: r.distribuidoraId,
      subgrupo: r.subgrupo,
      tarifaTe: r.tarifaTe,
      tarifaTusd: r.tarifaTusd,
      valorFioBPorKwh: r.valorFioBPorKwh,
      aliquotaIcms: r.aliquotaIcms,
      aliquotaPisCofins: r.aliquotaPisCofins,
      vigenciaInicio: r.vigenciaInicio,
      vigenciaFim: r.vigenciaFim,
      resolucaoHomologatoria: r.resolucaoHomologatoria,
      fonte: r.fonte,
    }),
  };
}
