import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { LucideAngularModule, Calendar, Search } from 'lucide-angular';
import { HistorySearchService } from '../../../../user-front/services/history-search.service';

@Component({
  selector: 'history-search',
  imports: [ReactiveFormsModule, LucideAngularModule],
  templateUrl: './history-search.component.html',
})
export class HistorySearchComponent {
   private fb = inject(FormBuilder);
  private router = inject(Router);
  private historySearchService = inject(HistorySearchService);

  searchForm = this.fb.group({
    bookTitle: [''],
    fromDate: [''],
    toDate: ['']
  });

  // Estado para dropdown de fecha (si lo quieres)
  dateFilterOpen = signal(false);

  ngOnInit() {
    // Inicializa el formulario con los valores de la URL
    const params = this.historySearchService.searchParams();
    this.searchForm.patchValue({
      bookTitle: params.BookTitle || '',
      fromDate: params.FromDate ? this.formatDateForInput(params.FromDate) : '',
      toDate: params.ToDate ? this.formatDateForInput(params.ToDate) : ''
    });
  }

  // Formatear fecha para input type="date"
  private formatDateForInput(date: Date): string {
    return new Date(date).toISOString().split('T')[0];
  }

  // Formatear fecha desde input a Date object
  private parseDateFromInput(dateString: string): Date {
    return new Date(dateString);
  }

  toggleDateFilter() {
    this.dateFilterOpen.update(open => !open);
  }

  onSubmit() {
    const { bookTitle, fromDate, toDate } = this.searchForm.value;

    // Convertir strings de fecha a objetos Date
    const fromDateObj = fromDate ? this.parseDateFromInput(fromDate) : null;
    const toDateObj = toDate ? this.parseDateFromInput(toDate) : null;

    this.router.navigate([], {
      queryParams: {
        bookTitle: bookTitle || null,
        fromDate: fromDateObj ? fromDateObj.toISOString() : null,
        toDate: toDateObj ? toDateObj.toISOString() : null,
        page: 1 // Resetear a primera página al buscar
      },
      queryParamsHandling: 'merge'
    });
  }

  clearFilters() {
    this.searchForm.patchValue({
      bookTitle: '',
      fromDate: '',
      toDate: ''
    });

    this.router.navigate([], {
      queryParams: {
        bookTitle: null,
        fromDate: null,
        toDate: null,
        page: 1
      },
      queryParamsHandling: 'merge'
    });
  }

  // Iconos
  readonly calendar = Calendar;
  readonly searchIcon = Search;
}
