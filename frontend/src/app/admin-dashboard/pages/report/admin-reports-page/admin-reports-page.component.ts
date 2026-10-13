import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ReportsService, ReportStatus, ReportType, ReportDto, UpdateReportDto } from '../../../../features/books/services/reports.service';
import { rxResource } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-admin-reports-page',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './admin-reports-page.component.html',
})
export class AdminReportsPageComponent {
  private reportsService = inject(ReportsService);
  private router = inject(Router);

  // Exponer enums al template
  ReportStatusEnum = ReportStatus;
  ReportTypeEnum = ReportType;

  // Señales para el estado
  currentFilter = signal<'pending' | 'all'>('pending');
  updatingReports = signal<Set<number>>(new Set());


  // Almacenar decisiones temporales
  reportDecisions: { [key: number]: ReportStatus } = {};
  reportNotes: { [key: number]: string } = {};

  // Resource para cargar los reportes
  reportsResource = rxResource({
    loader: () => {
      return this.currentFilter() === 'pending'
        ? this.reportsService.getReportsByStatus(ReportStatus.Pendiente)
        : this.reportsService.getAllReports({ PageSize: 50 });
    }
  });

  // Resource para el resumen
  summaryResource = rxResource({
    loader: () => this.reportsService.getReportSummary()
  });

  // Señales computadas
  reports = this.reportsResource.value;
  summary = this.summaryResource.value;

  // Señal computada para reportes filtrados
  filteredReports = () => {
    const allReports = this.reports();
    if (this.currentFilter() === 'pending') {
      return allReports!.filter(r => r.status === ReportStatus.Pendiente);
    }
    return allReports;
  };

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
    const baseClasses = 'badge badge-lg font-semibold';
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

  getTypeBadgeClass(type: ReportType): string {
    const baseClasses = 'badge badge-sm';
    switch (type) {
      case ReportType.Error:
        return `${baseClasses} badge-error`;
      case ReportType.ContenidoInapropiado:
        return `${baseClasses} badge-warning`;
      case ReportType.DerechosAutor:
        return `${baseClasses} badge-secondary`;
      case ReportType.Otro:
        return `${baseClasses} badge-ghost`;
      default:
        return `${baseClasses} badge-ghost`;
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

  // Cambiar filtro
  setFilter(filter: 'pending' | 'all') {
    this.currentFilter.set(filter);
    this.reloadReports();
  }

  // Recargar reportes
  reloadReports() {
    this.reportsResource.reload();
  }

  // Verificar si se está actualizando un reporte
  isUpdatingReport(id: number): boolean {
    return this.updatingReports().has(id);
  }

  // Actualizar estado de un reporte
  async updateReportStatus(report: ReportDto) {
    const decision = this.reportDecisions[report.id];
    const notes = this.reportNotes[report.id] || '';

    if (!decision) {
      alert('Por favor selecciona una decisión');
      return;
    }

    // Agregar a la lista de actualizaciones en curso
    this.updatingReports.update(set => new Set(set.add(report.id)));

    try {
      type NewType = UpdateReportDto;

      const updateDto: UpdateReportDto = {
        status: Number(decision),
        adminNotes: notes
      };

      await this.reportsService.updateReport(report.id, updateDto).toPromise();

      // Recargar los reportes
      this.reloadReports();

      // Limpiar los datos temporales
      delete this.reportDecisions[report.id];
      delete this.reportNotes[report.id];

      alert(`Reporte actualizado correctamente`);

    } catch (error) {
      console.error('Error actualizando reporte:', error);
      alert('Error al procesar el reporte');
    } finally {
      // Remover de la lista de actualizaciones en curso
      this.updatingReports.update(set => {
        set.delete(report.id);
        return new Set(set);
      });
    }
  }

  // Ver detalles del libro
  viewBookDetails(bookId: number) {
    this.router.navigate(['/library', bookId]);
  }
}
