import { ParamMap, Params } from '@angular/router';
import { TrackFilter, TrackQuery } from './track.models';

export const PAGE_SIZE = 10;
// The backend's Skip offset is a signed 32-bit integer.
export const MAX_PAGE = Math.floor(2_147_483_647 / PAGE_SIZE) + 1;
export const TRACK_FILTERS: { value: TrackFilter; label: string }[] = [
  { value: 'all', label: 'All tracks' },
  { value: 'draft', label: 'Draft' },
  { value: 'submitted', label: 'Submitted' },
  { value: 'distributed', label: 'Distributed' },
];

export function parseTrackQuery(params: ParamMap): TrackQuery {
  const status = params.get('status');
  const page = params.get('page') ?? '1';
  const pageNumber = /^[1-9]\d*$/.test(page) ? Number(page) : NaN;
  return {
    status: TRACK_FILTERS.find((filter) => filter.value === status)?.value ?? 'all',
    page: Number.isSafeInteger(pageNumber) && pageNumber <= MAX_PAGE ? pageNumber : 1,
  };
}

export function trackQueryParams(query: TrackQuery): Params {
  return {
    ...(query.status !== 'all' ? { status: query.status } : {}),
    ...(query.page !== 1 ? { page: query.page } : {}),
  };
}

export function sameTrackQuery(a: TrackQuery, b: TrackQuery): boolean {
  return a.status === b.status && a.page === b.page;
}

export function isTrackId(id: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(id);
}
