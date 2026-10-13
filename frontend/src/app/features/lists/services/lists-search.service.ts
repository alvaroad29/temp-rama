import { inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';
import { SortByBookList } from '../interfaces/book-list-books-query-parameters.interface';

@Injectable({
  providedIn: 'root'
})
export class ListsSearchService {

  private activatedRoute = inject(ActivatedRoute);

  searchParams = toSignal(
    this.activatedRoute.queryParamMap.pipe(
      map( params => {
        const genreIdsParam = params.get('genreIds');
        const GenreIds = genreIdsParam
          ? genreIdsParam.split(',').map(id => +id).filter(id => !isNaN(id))
          : [];

        const SearchText = params.get('searchText') || '';

        const SortDescending = params.get('sr') === 'true';

        const SortByRaw = params.get('sortBy');
        const SortByValue = this.isValidSortBy(SortByRaw) ? (SortByRaw as SortByBookList) : SortByBookList.Title;

        return {
          SearchText,
          SortBy: SortByValue,
          SortDescending,
          GenreIds,
        };
      })
    ),
    {
      initialValue: {
        SearchText: '',
        SortBy: SortByBookList.Title,
        SortDescending: false,
        GenreIds: [],
      }
    }
  );

    private isValidSortBy(value: string | null): boolean {
      return Object.values(SortByBookList).includes(value as SortByBookList);
    }


}
