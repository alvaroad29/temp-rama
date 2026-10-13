import { Component, computed, inject, signal } from '@angular/core';
import { BookUpdateCardComponent } from "../book-update-card/book-update-card.component";
import { BooksService } from '../../services/books.service';
import { rxResource } from '@angular/core/rxjs-interop';

@Component({
  selector: 'recent-updates',
  imports: [BookUpdateCardComponent],
  templateUrl: './ recent-updates.component.html',
})
export class RecentUpdatesComponent {

  private booksService = inject(BooksService);

  booksResource = rxResource({
      request: () => ({}),
      loader: ({request}) => {
        return this.booksService.getRecentBooksWithChapters();
    }
  }
  );

  recentBooks = computed(() => this.booksResource.value() ?? []);
  isLoading = computed(() => this.booksResource.isLoading());
  error = computed(() => this.booksResource.error());

}
