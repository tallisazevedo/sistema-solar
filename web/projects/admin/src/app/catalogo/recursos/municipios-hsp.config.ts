import { HttpClient } from '@angular/common/http';
import { firstValueFrom, map } from 'rxjs';
import {
  ApiConfiguration,
  MunicipioHspRequest,
  MunicipioHspResponse,
  apiTarifasDistribuidorasGet$Json,
  apiTarifasMunicipiosGet$Json,
  apiTarifasMunicipiosIdDelete,
  apiTarifasMunicipiosIdPut,
  apiTarifasMunicipiosPost$Json,
} from 'shared';
import { RecursoConfig } from '../crud-generico/crud-generico.types';
import { criarApiCrud } from './api-adapter';

export async function criarConfigMunicipiosHsp(
  http: HttpClient,
  apiConfig: ApiConfiguration,
): Promise<RecursoConfig<MunicipioHspResponse & { id: string }, MunicipioHspRequest>> {
  const distribuidoras = await firstValueFrom(
    apiTarifasDistribuidorasGet$Json(http, apiConfig.rootUrl).pipe(map((r) => r.body)),
  );
  const opcoesDistribuidora = distribuidoras.map((d) => ({ valor: d.id, rotulo: `${d.nome} (${d.siglaAneel})` }));

  return {
    titulo: 'Municipios (HSP)',
    campos: [
      { chave: 'codigoIbge', rotulo: 'Codigo IBGE', tipo: 'texto', obrigatorio: true },
      { chave: 'nome', rotulo: 'Nome', tipo: 'texto', obrigatorio: true },
      { chave: 'latitude', rotulo: 'Latitude', tipo: 'decimal', obrigatorio: true },
      { chave: 'longitude', rotulo: 'Longitude', tipo: 'decimal', obrigatorio: true },
      { chave: 'distanciaMarKm', rotulo: 'Distancia do mar (km)', tipo: 'decimal', obrigatorio: true },
      { chave: 'distribuidoraId', rotulo: 'Distribuidora', tipo: 'enum', opcoesEnum: opcoesDistribuidora, obrigatorio: true },
      { chave: 'hspPorMes', rotulo: 'HSP por mes (jan-dez)', tipo: 'hsp-mensal' },
      { chave: 'fonte', rotulo: 'Fonte', tipo: 'texto', obrigatorio: true },
    ],
    api: criarApiCrud(http, apiConfig.rootUrl, {
      listar: apiTarifasMunicipiosGet$Json,
      criar: apiTarifasMunicipiosPost$Json,
      atualizar: apiTarifasMunicipiosIdPut,
      remover: apiTarifasMunicipiosIdDelete,
    }),
    criarRequestVazio: () => ({
      codigoIbge: '',
      nome: '',
      latitude: 0,
      longitude: 0,
      distanciaMarKm: 0,
      distribuidoraId: opcoesDistribuidora[0]?.valor as string,
      hspPorMes: Array(12).fill(0),
      fonte: '',
    }),
    paraRequest: (r) => ({
      codigoIbge: r.codigoIbge,
      nome: r.nome,
      latitude: r.latitude,
      longitude: r.longitude,
      distanciaMarKm: r.distanciaMarKm,
      distribuidoraId: r.distribuidoraId,
      hspPorMes: [...r.hspPorMes],
      fonte: r.fonte,
    }),
  };
}
