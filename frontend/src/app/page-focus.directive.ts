import { afterNextRender, Directive, ElementRef, inject } from '@angular/core';

@Directive({ selector: '[appPageFocus]', host: { tabindex: '-1' } })
export class PageFocus {
  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);

  constructor() {
    afterNextRender(() => this.element.nativeElement.focus({ preventScroll: true }));
  }
}
