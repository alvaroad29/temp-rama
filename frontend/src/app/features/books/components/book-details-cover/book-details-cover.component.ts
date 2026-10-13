import { LucideAngularModule, Star } from 'lucide-angular';
import { Component, computed, input, signal } from '@angular/core';

import { RouterLink } from '@angular/router';
import { BookDetails } from '../../interfaces/book-details-interface';
import { State } from '../../interfaces/book-response.interface';
import { DecimalPipe } from '@angular/common';
import { BookRatingComponent } from '../book-rating/book-rating.component';


@Component({
  selector: 'book-details-cover',
  imports: [RouterLink, LucideAngularModule, DecimalPipe, BookRatingComponent],
  templateUrl: './book-details-cover.component.html',
})
export class BookDetailsCoverComponent {
  book = input<BookDetails>();

  private ratingUpdates = signal<{score: number, totalRatings: number} | null>(null);

  displayScore = computed(() => {
    const updates = this.ratingUpdates();
    if (updates) {
      return updates.score;
    }
    return this.book()?.score || 0;
  });

  displayTotalRatings = computed(() => {
    const updates = this.ratingUpdates();
    if (updates) {
      return updates.totalRatings;
    }
    return this.book()?.totalRatings || 0;
  });

  getStatus(state: number)
  {
    return State[state];
  }

  onRatingChanged(event: {newScore: number, newTotalRatings: number}) {
    this.ratingUpdates.set({
      score: event.newScore,
      totalRatings: event.newTotalRatings
    });
  }


  readonly star = Star;
}
