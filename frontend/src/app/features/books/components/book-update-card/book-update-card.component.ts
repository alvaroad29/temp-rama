import { Component, input } from '@angular/core';
import { SlicePipe, TitleCasePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time.pipe';
import { CardUpdate } from '../../interfaces/card-update.interface';

@Component({
  selector: 'book-update-card',
  imports: [RouterLink, RelativeTimePipe, TitleCasePipe, SlicePipe],
  templateUrl: './book-update-card.component.html',
})
export class BookUpdateCardComponent {
  book = input<CardUpdate>();
}
