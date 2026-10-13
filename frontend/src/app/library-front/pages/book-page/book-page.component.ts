import { Component, computed, inject, signal } from '@angular/core';
import { BookDetailsCoverComponent } from "../../../features/books/components/book-details-cover/book-details-cover.component";
import { ChapterAccordionComponent } from "../../../features/books/components/book-details-content/book-details-content.component";
import { rxResource } from '@angular/core/rxjs-interop';
import { BooksService } from '../../../features/books/services/books.service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AddToListComponent } from '../../../features/lists/components/add-to-list/add-to-list.component';

@Component({
  selector: 'app-book-page',
  imports: [BookDetailsCoverComponent, ChapterAccordionComponent, AddToListComponent, RouterLink],
  templateUrl: './book-page.component.html',
})
export class BookPageComponent {

  activatedRoute = inject(ActivatedRoute);

  private booksService = inject(BooksService);

  bookId: number = this.activatedRoute.snapshot.params['bookId'];

  bookResource = rxResource({
    request: () => ({id: this.bookId}),
    loader: ({request}) => {
        return this.booksService.getBook(request.id);
      },
    }
  );

  isLoading = computed(() => this.bookResource.isLoading());
  hasError = computed(() => this.bookResource.error());

}
