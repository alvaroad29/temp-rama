import { Component, signal } from '@angular/core';

@Component({
  selector: 'front-footer',
  imports: [],
  templateUrl: './front-footer.component.html',
})
export class FrontFooterComponent {

  date = signal(new Date().getFullYear());
}
