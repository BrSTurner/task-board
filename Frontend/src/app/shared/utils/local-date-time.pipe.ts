import { Pipe, PipeTransform } from '@angular/core';

import { formatLocalDateTime } from './date-time';

@Pipe({ name: 'localDateTime' })
export class LocalDateTimePipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    return formatLocalDateTime(value);
  }
}
