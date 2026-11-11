import { Routes } from '@angular/router';
import { TrackList } from './tracks/pages/track-list/track-list';
import { TrackDetail } from './tracks/pages/track-detail/track-detail';

export const routes: Routes = [
  { path: 'tracks', component: TrackList, title: 'Track catalogue · Takwene' },
  { path: 'tracks/:id', component: TrackDetail, title: 'Track details · Takwene' },
  { path: '', redirectTo: 'tracks', pathMatch: 'full' },
  { path: '**', redirectTo: 'tracks' },
];
