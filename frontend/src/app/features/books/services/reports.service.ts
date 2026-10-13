// shared/services/reports.service.ts (completo)
import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../../environments/environment';
import { map, Observable } from 'rxjs';

const baseUrl = environment.baseUrl;

export interface CreateReportDto {
  bookId: number;
  title: string;
  description: string;
  reportType: number;
}

export interface ReportDto {
  id: number;
  title: string;
  description: string;
  bookId: number;
  bookTitle: string;
  bookImageUrl: string;
  reporterId: string;
  reporterName: string;
  reporterEmail: string;
  status: ReportStatus;
  reportType: ReportType;
  reviewedByAdminId?: string;
  reviewedByAdminName?: string;
  reviewedAt?: string;
  adminNotes?: string;
  creationDate: string;
}

export interface PagedResponse<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalRecords: number;
  totalPages: number;
}

export interface ReportQueryParameters {
  PageNumber?: number;
  PageSize?: number;
  Status?: ReportStatus;
  ReportType?: ReportType;
  BookId?: number;
  ReporterId?: string;
  SortBy?: string;
  SortDescending?: boolean;
}

export interface ReportSummaryDto {
  totalReports: number;
  pendingReports: number;
  inReviewReports: number;
  resolvedReports: number;
}

export interface UpdateReportDto {
  status: ReportStatus;
  adminNotes?: string;
}

export enum ReportStatus {
  Pendiente = 1,
  EnRevision = 2,
  Resuelto = 3
}

export enum ReportType {
  Error = 1,
  ContenidoInapropiado = 2,
  DerechosAutor = 3,
  Otro = 4
}

export type ReportTypeOption = {
  value: ReportType;
  label: string;
};

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private http = inject(HttpClient);

  createReport(report: CreateReportDto): Observable<ReportDto> {
    const payload = {
      ...report,
      reportType: Number(report.reportType)
    };

    console.log('Payload enviado:', payload);
    return this.http.post<ReportDto>(`${baseUrl}/Reports`, payload);
  }

  getMyReports(options: ReportQueryParameters): Observable<ReportDto[]> {
    const defaultedOptions = {
      PageNumber: 1,
      PageSize: 10,
      SortDescending: true,
      SortBy: 'creationDate',
      ...options
    };

    const params = Object.fromEntries(
      Object.entries(defaultedOptions)
        .filter(([_, value]) => value !== undefined)
    );

    return this.http.get<PagedResponse<ReportDto>>(`${baseUrl}/Reports/my-reports`, { params })
      .pipe(
        map(response => response.data)
      );
  }
  getReportTypes(): ReportTypeOption[] {
    return [
      { value: ReportType.Error, label: 'Error en el contenido' },
      { value: ReportType.ContenidoInapropiado, label: 'Contenido inapropiado' },
      { value: ReportType.DerechosAutor, label: 'Problema de derechos de autor' },
      { value: ReportType.Otro, label: 'Otro' }
    ];
  }

  getAllReports(options?: ReportQueryParameters): Observable<ReportDto[]> {
  const defaultedOptions = {
    PageNumber: 1,
    PageSize: 50, // Número grande para obtener todos
    SortDescending: true,
    SortBy: 'creationDate',
    ...options
  };

  const params = Object.fromEntries(
    Object.entries(defaultedOptions)
      .filter(([_, value]) => value !== undefined)
  );

  return this.http.get<PagedResponse<ReportDto>>(`${baseUrl}/Reports`, { params })
    .pipe(
      map(response => response.data) // Extraemos solo el array de datos
    );
}

getReportsByStatus(status: ReportStatus): Observable<ReportDto[]> {
  const options: ReportQueryParameters = {
    Status: status,
    PageSize: 100 // Número grande para obtener todos
  };

  return this.getAllReports(options);
}

  getReportsByBook(bookId: number): Observable<ReportDto[]> {
    return this.http.get<ReportDto[]>(`${baseUrl}/Reports/book/${bookId}`);
  }

  getReportSummary(): Observable<ReportSummaryDto> {
    return this.http.get<ReportSummaryDto>(`${baseUrl}/Reports/summary`);
  }

  updateReport(id: number, updateDto: UpdateReportDto): Observable<ReportDto> {
    return this.http.put<ReportDto>(`${baseUrl}/Reports/${id}`, updateDto);
  }

  getReport(id: number): Observable<ReportDto> {
    return this.http.get<ReportDto>(`${baseUrl}/Reports/${id}`);
  }

  // Método auxiliar para obtener reportes sin paginación (para admin)
  getAllReportsSimple(): Observable<ReportDto[]> {
    return this.http.get<ReportDto[]>(`${baseUrl}/Reports/all`);
  }

  // Método para obtener reportes pendientes (uso común en admin)
  getPendingReports(): Observable<ReportDto[]> {
    return this.getReportsByStatus(ReportStatus.Pendiente);
  }
}
