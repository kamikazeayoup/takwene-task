import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { Distribution } from '../../data/track.models';
import { StatusLabel } from '../status-label/status-label';

const serviceIcons: Record<string, string> = {
  Spotify: '/assets/icons/spotify.svg',
  'Apple Music': '/assets/icons/applemusic.svg',
  YouTube: '/assets/icons/youtube.svg',
};

@Component({
  selector: 'app-distribution-map',
  imports: [DatePipe, StatusLabel],
  templateUrl: './distribution-map.html',
  styleUrl: './distribution-map.css',
})
export class DistributionMap {
  readonly trackLabel = input.required<string>();
  readonly distributions = input.required<Distribution[]>();

  iconFor(name: string): string | undefined {
    return Object.hasOwn(serviceIcons, name) ? serviceIcons[name] : undefined;
  }
}
