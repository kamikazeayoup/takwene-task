import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import {
  catchError,
  combineLatest,
  distinctUntilChanged,
  map,
  of,
  startWith,
  Subject,
  switchMap,
  tap,
} from 'rxjs';
import { PageFocus } from '../../../page-focus.directive';
import { LoadingIndicator } from '../../../shared/loading-indicator/loading-indicator';
import { DistributionMap } from '../../components/distribution-map/distribution-map';
import { ReleaseDatePipe } from '../../components/release-date.pipe';
import { StatusLabel } from '../../components/status-label/status-label';
import { TrackApi } from '../../data/track-api';
import { RequestState, TrackDetailResponse } from '../../data/track.models';
import { isTrackId, parseTrackQuery, trackQueryParams } from '../../data/track-query';

@Component({
  selector: 'app-track-detail',
  imports: [RouterLink, PageFocus, DistributionMap, ReleaseDatePipe, StatusLabel, LoadingIndicator],
  templateUrl: './track-detail.html',
  styleUrl: './track-detail.css',
})
export class TrackDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(TrackApi);
  private readonly title = inject(Title);
  private readonly retryRequest = new Subject<void>();
  readonly returnParams = toSignal(
    this.route.queryParamMap.pipe(map((params) => trackQueryParams(parseTrackQuery(params)))),
    { initialValue: {} },
  );

  readonly state = toSignal(
    combineLatest([
      this.route.paramMap.pipe(
        map((params) => params.get('id') ?? ''),
        distinctUntilChanged(),
      ),
      this.retryRequest.pipe(startWith(undefined)),
    ]).pipe(
      switchMap(([id]) => {
        this.title.setTitle('Track details · Takwene');
        if (!isTrackId(id)) return of<RequestState<TrackDetailResponse>>({ kind: 'missing' });
        return this.api.getTrack(id).pipe(
          tap((track) => this.title.setTitle(`${track.title} · Takwene`)),
          map((data): RequestState<TrackDetailResponse> => ({ kind: 'ready', data })),
          catchError((error: unknown) =>
            of<RequestState<TrackDetailResponse>>({
              kind:
                error instanceof HttpErrorResponse && error.status === 404 ? 'missing' : 'error',
            }),
          ),
          startWith<RequestState<TrackDetailResponse>>({ kind: 'loading' }),
        );
      }),
    ),
    { initialValue: { kind: 'loading' } as RequestState<TrackDetailResponse> },
  );

  retry(): void {
    this.retryRequest.next();
  }
}
