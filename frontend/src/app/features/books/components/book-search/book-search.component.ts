import { Component, computed, inject, signal, ViewChild, ElementRef, HostListener } from '@angular/core';
import { FormArray, FormBuilder, FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BooksService, SearchFieldOption, SearchInField, SortBy, SortByOption } from '../../services/books.service';
import { BookSearchService } from '../../services/book-search.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { State } from '../../interfaces/book-response.interface';
import { ChevronDown, LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'book-search',
  imports: [ReactiveFormsModule, LucideAngularModule],
  templateUrl: './book-search.component.html',
})
export class BookSearchComponent {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private bookSearchService = inject(BookSearchService);

  private booksService = inject(BooksService);


   // Resource para cargar géneros
  genresResource = rxResource({
      loader: () => {
          return this.booksService.getGenres();
        },
      }
    );

  // Señal computada para los géneros
  genres = computed(() => this.genresResource.value() ?? []);

  // Señal para los géneros seleccionados
  selectedGenreIds = signal<number[]>([]);

  searchForm = this.fb.group({
    searchText: [''],
    searchIn: [SearchInField.Title], // Campo por defecto: título
    sortBy: [SortBy.Title],
    sortDescending: [false],
    genreIds: this.fb.array<number>([]), // Array para los géneros seleccionados
    state: [State.All]
  });

  fields: SearchFieldOption[] = [
    { value: SearchInField.All, label: 'Todos los campos' },
    { value: SearchInField.Title, label: 'Título' },
    { value: SearchInField.Author, label: 'Autor' },
    { value: SearchInField.Description, label: 'Descripción' }
  ];

  sortOptions : SortByOption [] = [
    { value: SortBy.Title, label: 'Título' },
    { value: SortBy.Views, label: 'Vistas' },
    { value: SortBy.Score, label: 'Puntuacion' }
  ];

  orderOptions = [
    { value: false, label: 'Ascendente' },
    { value: true, label: 'Descendente' }
  ];

  stateOptions = [
    { value: State.All, label: 'Seleccionar...' },
    { value: State.Activo, label: 'Activo' },
    { value: State.Completado, label: 'Completado' },
    { value: State.Pausado, label: 'Pausado' }
  ];

  ngOnInit() {
    // Inicializa el formulario con los valores de la URL
    const params = this.bookSearchService.searchParams();
    this.searchForm.patchValue({
      searchText: params?.SearchText,
      searchIn: params?.SearchIn,
      sortBy: params?.SortBy || SortBy.Title,
      sortDescending: params?.SortDescending || false,
      state: params?.State || null
    });

    // Cargar géneros seleccionados desde la URL
    if (params?.GenreIds?.length) {
      this.setSelectedGenres(params.GenreIds);
    }
  }

  get genreControls(): FormArray {
    return this.searchForm.get('genreIds') as FormArray;
  }

  // En tu componente
  toggleGenre(genreId: number) {  // Usar number en lugar de string si ese es el tipo de ID
    const current = this.selectedGenreIds();
    const updated = current.includes(genreId)
      ? current.filter(id => id !== genreId)
      : [...current, genreId];

    this.selectedGenreIds.set(updated);

    // Actualizar el FormArray para mantener consistencia
    const genreArray = this.searchForm.get('genreIds') as FormArray;
    genreArray.clear();
    updated.forEach(id => genreArray.push(new FormControl(id)));
  }

  isGenreSelected(genreId: number): boolean {
    return this.selectedGenreIds().includes(genreId);
  }

  setSelectedGenres(genreIds: number[]) {
    const genreArray = this.genreControls;
    genreArray.clear();
    genreIds.forEach(id => genreArray.push(new FormControl(id)));
    this.selectedGenreIds.set(genreIds);
  }

  // dropdown
  dropdownOpen = signal(false);

  toggleDropdown() {
    this.dropdownOpen.update(open => !open);
  }


  onSubmit() {
    const { searchText, searchIn, sortBy, sortDescending, state} = this.searchForm.value;
    const genreIds = this.selectedGenreIds();

    this.router.navigate([], {
      queryParams: {
        searchText: searchText,
        searchIn: searchIn,
        sortBy: sortBy,
        sr: sortDescending,
        genreIds: genreIds.length ? genreIds.join(',') : null,
        state:  state === State.All ? undefined : state,
        page: 1
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
