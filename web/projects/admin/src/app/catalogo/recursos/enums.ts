import { OpcaoEnum } from '../crud-generico/crud-generico.types';

export const OPCOES_TIPO_TELHADO: OpcaoEnum[] = [
  { valor: 0, rotulo: 'Ceramico' },
  { valor: 1, rotulo: 'Metalico' },
  { valor: 2, rotulo: 'Fibrocimento' },
  { valor: 3, rotulo: 'Laje' },
  { valor: 4, rotulo: 'Solo' },
];

export const OPCOES_TIPO_INVERSOR: OpcaoEnum[] = [
  { valor: 0, rotulo: 'String' },
  { valor: 1, rotulo: 'Micro' },
];

export const OPCOES_SUBGRUPO: OpcaoEnum[] = [
  { valor: 0, rotulo: 'B1' },
  { valor: 1, rotulo: 'B2' },
  { valor: 2, rotulo: 'B3' },
];
