import { Meta } from '@angular/platform-browser';

export function configurarMetaSemOpenGraph(meta: Meta, description: string): void {
  meta.updateTag({ name: 'description', content: description });
  for (const property of ['og:type', 'og:title', 'og:description', 'og:image']) {
    meta.removeTag(`property="${property}"`);
  }
}
