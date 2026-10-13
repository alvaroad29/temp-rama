import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ListsService } from '../../../../features/lists/services/lists.service';
import { AuthService } from '../../../../auth/services/auth.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { map, of } from 'rxjs';
import { Router, RouterLink } from '@angular/router';
import { SortByBookList } from '../../../../features/lists/interfaces/book-list-books-query-parameters.interface';


@Component({
  selector: 'app-admin-home-carousel-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-home-carousel-page.component.html',
})
export class AdminHomeCarouselPageComponent {
  private listsService = inject(ListsService);
  private authService = inject(AuthService);
  private router = inject(Router);

  isLoading = signal(false);

  // Resource para la lista del carousel
  carouselResource = rxResource({
    loader: () => this.listsService.getOrCreateCarouselList()
  });

  // Resource para los libros del carousel
  booksResource = rxResource({
    request: () => ({ listId: this.carouselResource.value()?.id }),
    loader: ({ request }) => {
      if (!request.listId) return of([]);
      return this.listsService.getDetailedList(request.listId, {
        PageNumber: 1,
        PageSize: 10,
        SortBy: SortByBookList.Title,
        SortDescending: false
      }).pipe(
        map(response => response.books.data)
      );
    }
  });

  async createCarouselList() {
    this.isLoading.set(true);
    try {
      // Forzar la creación de la lista
      const list = await this.listsService.getOrCreateCarouselList().toPromise();
      this.carouselResource.reload();
    } catch (error) {
      console.error('Error creando lista del carousel:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  refreshCarousel() {
    this.carouselResource.reload();
    if (this.carouselResource.value()) {
      this.booksResource.reload();
    }
  }
}
