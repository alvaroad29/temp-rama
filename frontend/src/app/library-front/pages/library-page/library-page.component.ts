import { PaginationService } from './../../../shared/components/pagination/pagination.service';
import { Component, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { BooksService } from '../../../features/books/services/books.service';
import { BookCardComponent } from '../../../features/books/components/book-card/book-card.component';
import { Book, State } from '../../../features/books/interfaces/book-response.interface';
import { CardData } from '../../../features/books/interfaces/card-data.interface';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { BookSearchComponent } from '../../../features/books/components/book-search/book-search.component';
import { BookSearchService } from '../../../features/books/services/book-search.service';


@Component({
  selector: 'app-library-page',
  imports: [BookCardComponent, PaginationComponent, BookSearchComponent],
  templateUrl: './library-page.component.html',
})
export class LibraryPageComponent {
  private booksService = inject(BooksService);

  paginationService = inject(PaginationService);
  bookSearchService = inject(BookSearchService);

  booksResource = rxResource({
    request: () => ({
      page: this.paginationService.currentPage(),
      ...this.bookSearchService.searchParams()
    }),
    loader: ({request}) => {
      // console.log({request});

        return this.booksService.getBooks({
          PageNumber: request.page,
          SearchText: request.SearchText,
          SearchIn: request.SearchIn,
          SortBy: request.SortBy,
          SortDescending: request.SortDescending,
          GenreIds: request.GenreIds,
          State: request.State
        });
      },
    }
  );


  // Método para mapear Book[] → CardData[]
  private mapBooks(books: Book[]): CardData[] {
    return books.map(book => ({
      id: book.id.toString(),
      title: book.title,
      image: book.imageUrl,
      score: book.score,
      status: this.mapStateToLabel(book.state),
    }));
  }

  // Estado derivado: se actualiza automáticamente cuando llegan los datos
  data = computed(() => {
    const resource = this.booksResource.value();
    if (!resource?.data) return [];
    return this.mapBooks(resource.data);
  });

  // Helper para estados legibles
  private mapStateToLabel(state: State): string {
    switch (state) {
      case State.Activo:     return 'Activo';
      case State.Completado: return 'Completado';
      case State.Pausado:    return 'Pausado';
      default:               return 'Desconocido';
    }
  }

}
