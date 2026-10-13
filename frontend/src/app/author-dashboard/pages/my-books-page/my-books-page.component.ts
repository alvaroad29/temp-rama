import { Component, computed, inject } from '@angular/core';
import { BooksService, SearchInField } from '../../../features/books/services/books.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { Book, State } from '../../../features/books/interfaces/book-response.interface';
import { CardData } from '../../../features/books/interfaces/card-data.interface';
import { PaginationService } from '../../../shared/components/pagination/pagination.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { BookCardComponent } from '../../../features/books/components/book-card/book-card.component';
import { AuthService } from '../../../auth/services/auth.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-my-books-page',
  imports: [BookCardComponent, PaginationComponent, RouterLink],
  templateUrl: './my-books-page.component.html',
})
export class MyBooksPageComponent {
  private booksService = inject(BooksService);
  authService = inject(AuthService);
  paginationService = inject(PaginationService);

  user = computed(() => this.authService.user());

  booksResource = rxResource({
    request: () => ({
      page: this.paginationService.currentPage(),
      ...this.user()
    }),
    loader: ({request}) => {
      console.log({request});

        return this.booksService.getBooks({
          PageNumber: request.page,
          SearchText: request.displayName, // cambiar despues por userName
          SearchIn: SearchInField.Author,
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
