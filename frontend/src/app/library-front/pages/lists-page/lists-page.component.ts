import { Component, computed, inject } from '@angular/core';
import { ListsService } from '../../../features/lists/services/lists.service';
import { PaginationService } from '../../../shared/components/pagination/pagination.service';
import { ListsSearchService } from '../../../features/lists/services/lists-search.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { GeneralItemListComponent } from '../../../features/lists/components/general-item-list/general-item-list.component';
import { RouterLink } from '@angular/router';
import { BookListSearchComponent } from '../../../features/lists/components/book-list-search/book-list-search.component';

@Component({
  selector: 'app-lists-page',
  imports: [PaginationComponent, GeneralItemListComponent, RouterLink, BookListSearchComponent],
  templateUrl: './lists-page.component.html',
})
export class ListsPageComponent {
  private listsService = inject(ListsService);
  paginationService = inject(PaginationService);

  listsSearchService = inject(ListsSearchService);

  listsResource = rxResource({
    request: () => ({
      page: this.paginationService.currentPage(),
      ...this.listsSearchService.searchParams()
    }),
    loader: ({request}) => {
      // console.log({request});

        return this.listsService.getLists({
          PageNumber: request.page,
          SearchText: request.SearchText,
          SortBy: request.SortBy,
          SortDescending: request.SortDescending,
          GenreIds: request.GenreIds,
        });
      },
    }
  );

  // Computed signals para mejor rendimiento
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
}
