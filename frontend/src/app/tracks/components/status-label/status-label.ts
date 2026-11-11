import { Component, computed, input } from '@angular/core';

const trackLabels: Record<string, string> = {
  draft: 'Draft',
  submitted: 'Submitted',
  distributed: 'Distributed',
};
const distributionLabels: Record<string, string> = {
  pending: 'Pending',
  live: 'Live',
  rejected: 'Rejected',
};

@Component({
  selector: 'app-status-label',
  templateUrl: './status-label.html',
  styleUrl: './status-label.css',
})
export class StatusLabel {
  readonly status = input.required<string>();
  readonly context = input.required<'track' | 'distribution'>();
  readonly presentation = computed(() => {
    const labels = this.context() === 'track' ? trackLabels : distributionLabels;
    const known = Object.hasOwn(labels, this.status());
    return {
      label: known ? labels[this.status()] : 'Unknown status',
      tone: known ? this.status() : 'unknown',
    };
  });
}
