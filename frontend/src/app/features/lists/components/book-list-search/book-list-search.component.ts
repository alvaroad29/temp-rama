import { Component, computed, inject, signal, ViewChild, ElementRef, HostListener  } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { LucideAngularModule, ChevronDown } from 'lucide-angular';
import { BooksService } from '../../../books/services/books.service';

import { ListsSearchService } from '../../services/lists-search.service';
import { Router } from '@angular/router';
import { SortByBookList } from '../../interfaces/book-list-books-query-parameters.interface';

@Component({
  selector: 'book-list-search',
  imports: [ReactiveFormsModule, LucideAngularModule],
  templateUrl: './book-list-search.component.html',
})
export class BookListSearchComponent {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private listsSearchService = inject(ListsSearchService);
  private booksService = inject(BooksService);

  // Resource para cargar géneros
  genresResource = rxResource({
    loader: () => {
      return this.booksService.getGenres();
    },
  });

  // Señal computada para los géneros
  genres = computed(() => this.genresResource.value() ?? []);

  // Señal para los géneros seleccionados
  selectedGenreIds = signal<number[]>([]);

  searchForm = this.fb.group({
    searchText: [''],
    sortBy: [SortByBookList.Title],
    sortDescending: [false],
  });

  sortOptions = [
    { value: SortByBookList.Title, label: 'Título' },
    { value: SortByBookList.FollowersCount, label: 'Seguidores' },
    { value: SortByBookList.CreationDate, label: 'Fecha de creación' },
    { value: SortByBookList.UpdateDate, label: 'Fecha de actualización' },
    { value: SortByBookList.BookCount, label: 'Cantidad de libros' }
  ];

  orderOptions = [
    { value: false, label: 'Ascendente' },
    { value: true, label: 'Descendente' }
  ];

  // Dropdown state
  dropdownOpen = signal(false);

  ngOnInit() {
    // Inicializa el formulario con los valores de la URL
    const params = this.listsSearchService.searchParams();
    this.searchForm.patchValue({
      searchText: params.SearchText,
      sortBy: params.SortBy,
      sortDescending: params.SortDescending,
    });

    // Cargar géneros seleccionados desde la URL
    if (params.GenreIds?.length) {
      this.selectedGenreIds.set(params.GenreIds);
    }
  }

  toggleGenre(genreId: number) {
    const current = this.selectedGenreIds();
    const updated = current.includes(genreId)
      ? current.filter(id => id !== genreId)
      : [...current, genreId];

    this.selectedGenreIds.set(updated);
  }

  isGenreSelected(genreId: number): boolean {
    return this.selectedGenreIds().includes(genreId);
  }

  toggleDropdown() {
    this.dropdownOpen.update(open => !open);
  }

  onSubmit() {
    const { searchText, sortBy, sortDescending } = this.searchForm.value;
    const genreIds = this.selectedGenreIds();

    this.router.navigate([], {
      queryParams: {
        searchText: searchText || null,
        sortBy: sortBy,
        sr: sortDescending,
        genreIds: genreIds.length ? genreIds.join(',') : null,
        page: 1 // Resetear a primera página al buscar
      },
      queryParamsHandling: 'merge'
    });
  }

  @ViewChild('genreDropdownContainer') genreDropdownContainer!: ElementRef;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    if (!this.dropdownOpen()) return;

    const container = this.genreDropdownContainer?.nativeElement;
    if (container && !container.contains(event.target)) {
      this.dropdownOpen.set(false);
    }
  }

  // Iconos
  readonly arrow = ChevronDown;
}
