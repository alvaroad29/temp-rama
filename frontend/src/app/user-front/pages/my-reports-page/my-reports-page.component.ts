import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ReportsService, ReportStatus, ReportType, ReportDto } from '../../../features/books/services/reports.service';
import { PagedResponse } from '../../../features/user/interfaces/user-query-params';

@Component({
  selector: 'app-my-reports-page',
  imports: [],
  templateUrl: './my-reports-page.component.html',
})
export class MyReportsPageComponent {
  private reportsService = inject(ReportsService);

  // Exponer enums al template
  ReportStatusEnum = ReportStatus;
  ReportTypeEnum = ReportType;

  // Señales para el estado
  currentPage = signal(1);
  pageSize = signal(10);
  isLoadingMore = signal(false);

  // Resource para cargar los reportes
  reportsResource = rxResource({
    loader: () => {
      const options = {
        PageNumber: this.currentPage(),
        PageSize: this.pageSize(),
        SortDescending: true,
        SortBy: 'creationDate'
      };

      return this.reportsService.getMyReports(options);
    }
  });

  // Señales computadas
  reports = this.reportsResource.value;
  pagedResponse = signal<PagedResponse<ReportDto> | null>(null);

  // Métodos de utilidad
  getStatusText(status: ReportStatus): string {
    switch (status) {
      case ReportStatus.Pendiente: return 'Pendiente';
      case ReportStatus.EnRevision: return 'En Revisión';
      case ReportStatus.Resuelto: return 'Resuelto';
      default: return 'Desconocido';
    }
  }

  getStatusBadgeClass(status: ReportStatus): string {
    const baseClasses = 'badge badge-sm font-semibold';
    switch (status) {
      case ReportStatus.Pendiente:
        return `${baseClasses} badge-warning`;
      case ReportStatus.EnRevision:
        return `${baseClasses} badge-info`;
      case ReportStatus.Resuelto:
        return `${baseClasses} badge-success`;
      default:
        return `${baseClasses} badge-ghost`;
    }
  }

  getTypeText(type: ReportType): string {
    switch (type) {
      case ReportType.Error: return 'Error';
      case ReportType.ContenidoInapropiado: return 'Contenido Inapropiado';
      case ReportType.DerechosAutor: return 'Derechos de Autor';
      case ReportType.Otro: return 'Otro';
      default: return 'Desconocido';
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // Recargar reportes
  reloadReports() {
    this.reportsResource.reload();
  }

  // Cargar más reportes (paginación)
  loadMore() {
    if (this.isLoadingMore()) return;

    this.isLoadingMore.set(true);
    this.currentPage.update(page => page + 1);

    const options = {
      PageNumber: this.currentPage(),
      PageSize: this.pageSize(),
      SortDescending: true,
      SortBy: 'creationDate'
    };

    this.reportsService.getMyReports(options).subscribe({
      next: (newReports) => {
        const currentReports = this.reports() || [];
        this.reports.set([...currentReports, ...newReports]);
        this.isLoadingMore.set(false);
      },
      error: (error) => {
        console.error('Error cargando más reportes:', error);
        this.isLoadingMore.set(false);
        // Revertir el incremento de página
        this.currentPage.update(page => page - 1);
      }
    });
  }

  // Ver si hay más páginas (simplificado)
  hasMoreReports(): boolean {
    const currentReports = this.reports() || [];
    return currentReports.length >= this.pageSize() * this.currentPage();
  }
}
