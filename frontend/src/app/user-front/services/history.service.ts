import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

const baseUrl = environment.baseUrl;

export interface HistoryQueryParameters {
  PageNumber?: number;
  PageSize?: number;
  BookTitle?: string;
  FromDate?: Date;
  ToDate?: Date;
}

export interface HistoryResponse {
  data: ReadingHistoryDto[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
}

export interface ReadingHistoryDto {
  chapterId: number;
  chapterTitle: string;
  bookId: number;
  bookTitle: string;
  imageUrl: string;
  viewDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class HistoryService {
  private http = inject(HttpClient);

  getHistory(options: HistoryQueryParameters): Observable<HistoryResponse> {
    const defaultedOptions = {
      PageNumber: 1,
      PageSize: 10,
      ...options
    };

    // Filtrar propiedades no deseadas
    const filteredOptions = Object.fromEntries(
      Object.entries(defaultedOptions)
        .filter(([_, value]) => {
          if (value === undefined || value === null || value === '') return false;
          return true;
        })
    );

    // Crear HttpParams
    let params = new HttpParams();

    Object.entries(filteredOptions).forEach(([key, value]) => {
      if (value instanceof Date) {
        params = params.set(key, value.toISOString());
      } else {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<HistoryResponse>(`${baseUrl}/history`, { params });
  }
}
