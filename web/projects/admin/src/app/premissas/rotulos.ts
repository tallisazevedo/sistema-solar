export const ROTULOS_PREMISSA = {
  performanceRatio: 'Performance Ratio (PR)',
  degradacaoAnual: 'Degradacao anual do modulo',
  inflacaoTarifaria: 'Inflacao tarifaria projetada',
  taxaDesconto: 'Taxa de desconto (VPL)',
  horizonteAnos: 'Horizonte do projeto (anos)',
  oversizingMaximo: 'Oversizing maximo',
  fatorOrientacaoPadrao: 'Fator de orientacao/inclinacao padrao',
  cronogramaFioB: 'Cronograma do Fio B (Lei 14.300)',
  estrategiaFioBForaCronograma: 'Estrategia de Fio B fora do cronograma',
  limiteKwpRoteamentoHumano: 'Limite de kWp para roteamento humano',
  kitLitoral: 'Kit litoral (municipios e raio)',
  custoDisponibilidadePorLigacao: 'Custo de disponibilidade por ligacao',
  textosProposta: 'Textos da proposta (disclaimer e validade)',
} as const;

export type ChavePremissa = keyof typeof ROTULOS_PREMISSA;

export const CHAVES_PREMISSA = Object.keys(ROTULOS_PREMISSA) as ChavePremissa[];

export const ORIGEM_PREMISSA_ROTULO = ['Lei', 'Fonte publica', 'Provisorio'] as const;

export const ORIGEM_PROVISORIO = 2;
