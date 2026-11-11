import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'releaseDate' })
export class ReleaseDatePipe implements PipeTransform {
  transform(value: string): string {
    const parts = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
    if (!parts) return 'Date unavailable';
    const [year, month, day] = parts.slice(1).map(Number);
    // Calendar dates stay in UTC; a local-time conversion can change the release day.
    const date = new Date(`${value}T00:00:00Z`);
    if (
      year < 1 ||
      !Number.isFinite(date.getTime()) ||
      date.getUTCFullYear() !== year ||
      date.getUTCMonth() + 1 !== month ||
      date.getUTCDate() !== day
    )
      return 'Date unavailable';
    return new Intl.DateTimeFormat('en-GB', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
      timeZone: 'UTC',
    }).format(date);
  }
}
