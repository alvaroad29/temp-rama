import { Component, inject, signal } from '@angular/core';
import { BooksService, BookSubmissionResponseDto, BookSubmissionStatus, UpdateBookSubmissionStatusDto } from '../../../../features/books/services/books.service';
import { Router, RouterLink } from '@angular/router';
import { rxResource } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-submissions-page',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './admin-submissions-page.component.html',
})
export class AdminSubmissionsPageComponent {
  private booksService = inject(BooksService);
  private router = inject(Router);

  // Exponer el enum al template
  BookSubmissionStatusEnum = BookSubmissionStatus;

  // Señales para el estado
  currentFilter = signal<'pending' | 'all'>('pending');
  updatingSubmissions = signal<Set<number>>(new Set());

  // Almacenar decisiones temporales
  submissionDecisions: { [key: number]: BookSubmissionStatus } = {};
  submissionFeedback: { [key: number]: string } = {};

  // Resource para cargar las submissions
  submissionsResource = rxResource({
    loader: () => {
      return this.currentFilter() === 'pending'
        ? this.booksService.getPendingSubmissions()
        : this.booksService.getAllSubmissions();
    }
  });

  // Señal computada para las submissions
  submissions = this.submissionsResource.value;

  // Señal computada para submissions filtradas
  filteredSubmissions = () => {
    const allSubmissions = this.submissions();
    if (this.currentFilter() === 'pending') {
      return allSubmissions!.filter(s => s.status === BookSubmissionStatus.Pending);
    }
    return allSubmissions;
  };

  // Resource para las estadísticas globales (siempre actual)
  statsResource = rxResource({
    loader: () => this.booksService.getSubmissionStats()
  });

  // Señal computada para las stats
  stats = this.statsResource.value;

  pendingCount = () => this.stats()?.['Pending'] ?? 0;
  approvedCount = () => this.stats()?.['Approved'] ?? 0;
  rejectedCount = () => this.stats()?.['Rejected'] ?? 0;
  needsChangesCount = () => this.stats()?.['NeedsChanges'] ?? 0;

  // Métodos de utilidad
  getStatusText(status: BookSubmissionStatus): string {
    switch (status) {
      case BookSubmissionStatus.Pending: return 'Pendiente';
      case BookSubmissionStatus.Approved: return 'Aprobado';
      case BookSubmissionStatus.Rejected: return 'Rechazado';
      case BookSubmissionStatus.NeedsChanges: return 'Requiere Cambios';
      default: return 'Desconocido';
    }
  }

  getStatusBadgeClass(status: BookSubmissionStatus): string {
    const baseClasses = 'badge badge-lg font-semibold';
    switch (status) {
      case BookSubmissionStatus.Pending:
        return `${baseClasses} badge-warning`;
      case BookSubmissionStatus.Approved:
        return `${baseClasses} badge-success`;
      case BookSubmissionStatus.Rejected:
        return `${baseClasses} badge-error`;
      case BookSubmissionStatus.NeedsChanges:
        return `${baseClasses} badge-info`;
      default:
        return `${baseClasses} badge-ghost`;
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  // Cambiar filtro
  setFilter(filter: 'pending' | 'all') {
    this.currentFilter.set(filter);
    this.reloadSubmissions();
  }

  // Recargar submissions
  reloadSubmissions() {
    this.submissionsResource.reload();
  }

  // Verificar si se está actualizando una submission
  isUpdatingSubmission(id: number): boolean {
    return this.updatingSubmissions().has(id);
  }

  // Actualizar estado de una submission
  async updateSubmissionStatus(submission: BookSubmissionResponseDto) {
    const decision = this.submissionDecisions[submission.id];
    const feedback = this.submissionFeedback[submission.id] || '';

    if (!decision) {
      alert('Por favor selecciona una decisión');
      return;
    }

    // Agregar a la lista de actualizaciones en curso
    this.updatingSubmissions.update(set => new Set(set.add(submission.id)));

    try {
     const updateDto: UpdateBookSubmissionStatusDto = {
      status: Number(decision) as BookSubmissionStatus,
      adminFeedback: feedback
    };
      console.log(updateDto);
      await this.booksService.updateSubmissionStatus(submission.id, updateDto).toPromise();

      // Recargar las submissions
      this.reloadSubmissions();
      this.statsResource.reload();

      // Limpiar los datos temporales
      delete this.submissionDecisions[submission.id];
      delete this.submissionFeedback[submission.id];

      alert(`Solicitud actualizada correctamente`);

    } catch (error) {
      console.error('Error actualizando submission:', error);
      alert('Error al procesar la solicitud');
    } finally {
      // Remover de la lista de actualizaciones en curso
      this.updatingSubmissions.update(set => {
        set.delete(submission.id);
        return new Set(set);
      });
    }
  }

  viewSubmissionDetail(id: number) {
    this.router.navigate(['/admin/submissions', id]);
  }
}
