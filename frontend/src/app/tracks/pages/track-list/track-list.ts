import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  catchError,
  combineLatest,
  distinctUntilChanged,
  map,
  of,
  startWith,
  Subject,
  switchMap,
} from 'rxjs';
import { PageFocus } from '../../../page-focus.directive';
import { LoadingIndicator } from '../../../shared/loading-indicator/loading-indicator';
import { SignalArtwork } from '../../../shared/signal-artwork/signal-artwork';
import { StatusLabel } from '../../components/status-label/status-label';
import { TrackApi } from '../../data/track-api';
import { PagedTracks, RequestState, TrackFilter, TrackQuery } from '../../data/track.models';
import {
  parseTrackQuery,
  sameTrackQuery,
  TRACK_FILTERS,
  trackQueryParams,
} from '../../data/track-query';

type CatalogueView = { query: TrackQuery; state: RequestState<PagedTracks> };

@Component({
  selector: 'app-track-list',
  imports: [RouterLink, PageFocus, StatusLabel, LoadingIndicator, SignalArtwork],
  templateUrl: './track-list.html',
  styleUrl: './track-list.css',
})
export class TrackList {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(TrackApi);
  private readonly retryRequest = new Subject<void>();
  readonly filters = TRACK_FILTERS;
  readonly queryParams = trackQueryParams;

  readonly view = toSignal(
    combineLatest([
      this.route.queryParamMap.pipe(map(parseTrackQuery), distinctUntilChanged(sameTrackQuery)),
      this.retryRequest.pipe(startWith(undefined)),
    ]).pipe(
      switchMap(([query]) =>
        this.api.getTracks(query).pipe(
          map((data): CatalogueView => ({ query, state: { kind: 'ready', data } })),
          catchError(() => of<CatalogueView>({ query, state: { kind: 'error' } })),
          startWith<CatalogueView>({ query, state: { kind: 'loading' } }),
        ),
      ),
    ),
    {
      initialValue: {
        query: parseTrackQuery(this.route.snapshot.queryParamMap),
        state: { kind: 'loading' },
      } as CatalogueView,
    },
  );

  selectFilter(status: TrackFilter): void {
    void this.router.navigate(['/tracks'], { queryParams: trackQueryParams({ status, page: 1 }) });
  }

  selectPage(page: number): void {
    void this.router.navigate(['/tracks'], {
      queryParams: trackQueryParams({ ...this.view().query, page }),
    });
  }

  retry(): void {
    this.retryRequest.next();
  }

  filterIndex(): number {
    return this.filters.findIndex((filter) => filter.value === this.view().query.status);
  }
}
