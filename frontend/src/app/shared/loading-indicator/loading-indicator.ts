import { Component } from '@angular/core';

@Component({
  selector: 'app-loading-indicator',
  template: '<span></span><span></span><span></span><span></span><span></span>',
  styleUrl: './loading-indicator.css',
  host: { 'aria-hidden': 'true' },
})
export class LoadingIndicator {}
