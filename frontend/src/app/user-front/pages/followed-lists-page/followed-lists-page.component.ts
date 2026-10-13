import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { GeneralItemListComponent } from '../../../features/lists/components/general-item-list/general-item-list.component';
import { ListsSearchService } from '../../../features/lists/services/lists-search.service';
import { ListsService } from '../../../features/lists/services/lists.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { PaginationService } from '../../../shared/components/pagination/pagination.service';
import { BookListSearchComponent } from '../../../features/lists/components/book-list-search/book-list-search.component';

@Component({
  selector: 'app-followed-lists-page',
  imports: [PaginationComponent, GeneralItemListComponent, RouterLink, BookListSearchComponent],
  templateUrl: './followed-lists-page.component.html',
})
export class FollowedListsPageComponent {
   private listsService = inject(ListsService);
  paginationService = inject(PaginationService);
  listsSearchService = inject(ListsSearchService);

  // Señal para mensajes
  successMessage = signal<string | null>(null);

  listsResource = rxResource({
    request: () => ({
      page: this.paginationService.currentPage(),
      ...this.listsSearchService.searchParams()
    }),
    loader: ({request}) => {
      console.log({request});
      return this.listsService.getFollowedLists({
        PageNumber: request.page,
        SearchText: request.SearchText,
        SortBy: request.SortBy,
        SortDescending: request.SortDescending,
        GenreIds: request.GenreIds,
      });
    },
  });

  // Computed signals
  lists = computed(() => {
    const result = this.listsResource.value();
    return result?.data || [];
  });

  totalPages = computed(() => {
    const result = this.listsResource.value();
    return result?.totalPages || 0;
  });

  isLoading = computed(() => this.listsResource.isLoading());
  error = computed(() => this.listsResource.error());

  onUnfollowedList(listId: number): void {
    // Recargar las listas
    this.listsResource.reload();

    // Mostrar mensaje de éxito
    this.successMessage.set('Lista dejada de seguir correctamente');

    // Limpiar mensaje después de 3 segundos
    setTimeout(() => {
      this.successMessage.set(null);
    }, 3000);
  }
}
