import { Pipe, type PipeTransform } from '@angular/core';
import { formatDistanceToNowStrict } from 'date-fns';
import { es } from 'date-fns/locale';

@Pipe({
  name: 'relativeTime',
})
export class RelativeTimePipe implements PipeTransform {

  transform(value: Date | string): string {
    if (!value) return '';

    const date = typeof value === 'string' ? new Date(value) : value;

    const distance =  formatDistanceToNowStrict(date, {

      addSuffix: true,
      locale: es, // Opcional: traduce a español
    });

     return distance
      .replace('minutos', 'min.')
      .replace('minuto', 'min.')
  }

}
