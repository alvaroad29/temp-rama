import { inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';

export interface HistorySearchParams {
  BookTitle?: string;
  FromDate?: Date;
  ToDate?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class HistorySearchService {
  private activatedRoute = inject(ActivatedRoute);

  searchParams = toSignal(
    this.activatedRoute.queryParamMap.pipe(
      map(params => {
        const bookTitle = params.get('bookTitle') || '';

        // Parsear fechas
        const fromDateParam = params.get('fromDate');
        const toDateParam = params.get('toDate');

        const fromDate = fromDateParam ? new Date(fromDateParam) : undefined;
        const toDate = toDateParam ? new Date(toDateParam) : undefined;

        // Validar que las fechas sean válidas
        const validFromDate = fromDate && !isNaN(fromDate.getTime()) ? fromDate : undefined;
        const validToDate = toDate && !isNaN(toDate.getTime()) ? toDate : undefined;

        return {
          BookTitle: bookTitle,
          FromDate: validFromDate,
          ToDate: validToDate
        };
      })
    ),
    {
      initialValue: {
        BookTitle: '',
        FromDate: undefined,
        ToDate: undefined
      }
    }
  );
}
