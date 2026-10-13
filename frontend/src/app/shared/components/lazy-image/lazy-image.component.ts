import { Component, signal } from '@angular/core';

@Component({
  selector: 'shared-lazy-image',
  imports: [],
  templateUrl: './lazy-image.component.html',
})
export class LazyImageComponent {

  hasLoaded = signal<boolean>(false);

  onLoad( ) {
    this.hasLoaded.set(true);
  }
}
