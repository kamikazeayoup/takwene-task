export type TrackStatus = 'draft' | 'submitted' | 'distributed';
export type DistributionStatus = 'pending' | 'live' | 'rejected';
export type TrackFilter = 'all' | TrackStatus;

export interface TrackQuery {
  status: TrackFilter;
  page: number;
}

export interface TrackSummary {
  id: string;
  title: string;
  artistId: string;
  artistName: string;
  isrc: string;
  releaseDate: string;
  genre: string;
  // Future server values remain visible through the status label's neutral fallback.
  status: TrackStatus | (string & {});
}

export interface Distribution {
  dspId: string;
  dspName: string;
  status: DistributionStatus | (string & {});
  submittedAt: string;
}

export interface TrackDetailResponse extends TrackSummary {
  distributions: Distribution[];
}

export interface PagedTracks {
  items: TrackSummary[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export type RequestState<T> =
  { kind: 'loading' } | { kind: 'ready'; data: T } | { kind: 'error' } | { kind: 'missing' };
