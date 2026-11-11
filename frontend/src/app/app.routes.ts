import { Routes } from '@angular/router';
import { TrackList } from './tracks/pages/track-list/track-list';

export const routes: Routes = [
  { path: 'tracks', component: TrackList, title: 'Track catalogue · Takwene' },
  { path: '', redirectTo: 'tracks', pathMatch: 'full' },
  { path: '**', redirectTo: 'tracks' },
];
