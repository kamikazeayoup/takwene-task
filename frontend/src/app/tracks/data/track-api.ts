import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import {
  Distribution,
  PagedTracks,
  TrackDetailResponse,
  TrackQuery,
  TrackSummary,
} from './track.models';
import { PAGE_SIZE } from './track-query';

function record(value: unknown): value is Record<string, unknown> {
  return value !== null && typeof value === 'object';
}

function hasStrings(value: Record<string, unknown>, keys: string[]): boolean {
  return keys.every((key) => typeof value[key] === 'string');
}

function isTrack(value: unknown): value is TrackSummary {
  return (
    record(value) &&
    hasStrings(value, [
      'id',
      'title',
      'artistId',
      'artistName',
      'isrc',
      'releaseDate',
      'genre',
      'status',
    ])
  );
}

function isDistribution(value: unknown): value is Distribution {
  return (
    record(value) &&
    hasStrings(value, ['dspId', 'dspName', 'status', 'submittedAt']) &&
    typeof value['submittedAt'] === 'string' &&
    /(?:Z|[+-]\d{2}:\d{2})$/i.test(value['submittedAt']) &&
    Number.isFinite(Date.parse(value['submittedAt']))
  );
}

function isPagedTracks(value: unknown): value is PagedTracks {
  if (!record(value)) return false;
  return (
    Array.isArray(value['items']) &&
    value['items'].every(isTrack) &&
    ['totalCount', 'totalPages'].every(
      (key) => Number.isSafeInteger(value[key]) && Number(value[key]) >= 0,
    ) &&
    ['pageNumber', 'pageSize'].every(
      (key) => Number.isSafeInteger(value[key]) && Number(value[key]) > 0,
    ) &&
    typeof value['hasPreviousPage'] === 'boolean' &&
    typeof value['hasNextPage'] === 'boolean'
  );
}

@Injectable({ providedIn: 'root' })
export class TrackApi {
  private readonly http = inject(HttpClient);

  getTracks(query: TrackQuery): Observable<PagedTracks> {
    let params = new HttpParams().set('pageNumber', query.page).set('pageSize', PAGE_SIZE);
    if (query.status !== 'all') params = params.set('status', query.status);
    return this.http.get<unknown>('/api/tracks', { params }).pipe(
      map((response) => {
        if (!isPagedTracks(response)) throw new Error('Invalid track collection response.');
        return response;
      }),
    );
  }

  getTrack(id: string): Observable<TrackDetailResponse> {
    return this.http.get<unknown>(`/api/tracks/${encodeURIComponent(id)}`).pipe(
      map((response) => {
        if (
          !isTrack(response) ||
          !record(response) ||
          !Array.isArray(response['distributions']) ||
          !response['distributions'].every(isDistribution)
        )
          throw new Error('Invalid track detail response.');
        return response as unknown as TrackDetailResponse;
      }),
    );
  }
}
