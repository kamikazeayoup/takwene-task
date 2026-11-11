import { Component } from '@angular/core';

@Component({
  selector: 'app-signal-artwork',
  templateUrl: './signal-artwork.html',
  styleUrl: './signal-artwork.css',
  host: { 'aria-hidden': 'true' },
})
export class SignalArtwork {
  // A fixed decorative pattern; it does not represent audio playback or delivery activity.
  readonly barHeights = [8, 16, 28, 40, 24, 52, 68, 44, 84, 60, 36, 72, 52, 28, 44, 20, 12];
}
