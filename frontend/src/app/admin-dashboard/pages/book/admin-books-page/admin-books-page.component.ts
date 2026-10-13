import { Component, computed, inject, signal } from '@angular/core';
import { Book, State } from '../../../../features/books/interfaces/book-response.interface';
import { rxResource } from '@angular/core/rxjs-interop';
import { BookQueryParameters, BooksService } from '../../../../features/books/services/books.service';
import { PaginationService } from '../../../../shared/components/pagination/pagination.service';

@Component({
  selector: 'app-admin-books-page',
  imports: [],
  templateUrl: './admin-books-page.component.html',
})
export class AdminBooksPageComponent {
  private booksService = inject(BooksService);
  paginationService = inject(PaginationService);

  // Estados reactivos
  searchText = signal('');
  isUpdating = signal<number | null>(null);

  // Resource para cargar libros
  booksResource = rxResource({
    request: () => ({
      page: this.paginationService.currentPage(),
      searchText: this.searchText()
    }),
    loader: ({ request }) => {
      const params: BookQueryParameters = {
        PageNumber: request.page,
        PageSize: 30,
        SearchText: request.searchText || undefined
      };
      return this.booksService.getBooksForAdmin(params);
    }
  });

  // Datos computados
  books = () => this.booksResource.value()?.data || [];
  totalRecords = () => this.booksResource.value()?.totalRecords || 0;

  // Señales computadas para estadísticas
  activeBooksCount = computed(() => {
    const booksList = this.books();
    return booksList.filter((b: Book) => b.isActive).length;
  });

  inactiveBooksCount = computed(() => {
    const booksList = this.books();
    return booksList.filter((b: Book) => !b.isActive).length;
  });

  // Métodos de utilidad
  getStateText(state: State): string {
    switch (state) {
      case State.Activo: return 'Activo';
      case State.Completado: return 'Completado';
      case State.Pausado: return 'Pausado';
      default: return 'Desconocido';
    }
  }

  getStateBadgeClass(state: State): string {
    const baseClasses = 'badge badge-sm';
    switch (state) {
      case State.Activo: return `${baseClasses} badge-success`;
      case State.Completado: return `${baseClasses} badge-info`;
      case State.Pausado: return `${baseClasses} badge-warning`;
      default: return `${baseClasses} badge-ghost`;
    }
  }

  getActiveBadgeClass(isActive: boolean): string {
    return isActive ? 'badge badge-sm badge-success' : 'badge badge-sm badge-error';
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('es-ES');
  }

  // Métodos de búsqueda
  onSearch(text: string) {
    this.searchText.set(text);

  }

  clearSearch() {
    this.searchText.set('');

  }


  deleteBook(book: Book) {
     if (!confirm(`¿Estás seguro de que quieres ${book.isActive ? 'desactivar' : 'activar'} el libro "${book.title}"?`)) {
      return;
    }

    // console.log();

    this.toggleBookStatus(book);
  }
  // Métodos de activación/desactivación
  async toggleBookStatus(book: Book) {
    if (this.isUpdating() !== null) return;

    this.isUpdating.set(book.id);
    

    try {
      const newStatus = !book.isActive;
      await this.booksService.deactivateBook(book.id, newStatus).toPromise();

      // Recargar los datos
      this.booksResource.reload();

    } catch (error) {
      console.error('Error actualizando estado del libro:', error);
      alert('Error al actualizar el estado del libro');
    } finally {
      this.isUpdating.set(null);
    }
  }

  // Recargar datos
  reloadData() {
    this.booksResource.reload();
  }
}
