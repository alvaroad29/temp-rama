import { inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';
import { SearchInField, SortBy } from './books.service';
import { State } from '../interfaces/book-response.interface';


@Injectable({
  providedIn: 'root'
})
export class BookSearchService {
  private activatedRoute = inject(ActivatedRoute);

  searchParams = toSignal(
    this.activatedRoute.queryParamMap.pipe(
      map(params => {
        const genreIdsParam = params.get('genreIds');
        const GenreIds = genreIdsParam
          ? genreIdsParam.split(',').map(id => +id).filter(id => !isNaN(id))
          : [];

        const stateParam = params.get('state');
        const StateValue = stateParam ? +stateParam : State.All;

        const SearchText = params.get('searchText') || '';
        const SearchInRaw = params.get('searchIn');
        const SearchIn = this.isValidSearchIn(SearchInRaw) ? (SearchInRaw as SearchInField) : SearchInField.Title;

        const SortByRaw = params.get('sortBy');
        const SortByValue = this.isValidSortBy(SortByRaw) ? (SortByRaw as SortBy) : SortBy.Title;

        const SortDescending = params.get('sr') === 'true';

        return {
          SearchText,
          SearchIn,
          SortBy: SortByValue,
          SortDescending,
          GenreIds,
          State: this.isValidState(StateValue) ? StateValue : undefined
        };
      })
    ),
    { initialValue: {
      SearchText: '',
      SearchIn: SearchInField.Title,
      SortBy: SortBy.Title,
      SortDescending: false,
      GenreIds: [],
      State: State.All
    } }
  );

  private isValidSearchIn(value: string | null): boolean {
    return Object.values(SearchInField).includes(value as SearchInField);
  }

  private isValidSortBy(value: string | null): boolean {
    return Object.values(SortBy).includes(value as SortBy);
  }

  private isValidState(value: number | null): value is State {
  return value !== null && Object.values(State).includes(value);
}
}
