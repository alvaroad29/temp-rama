import { Component, inject } from '@angular/core';
import { BooksService, BookSubmissionStatus } from '../../../features/books/services/books.service';
import { Router, RouterLink } from '@angular/router';
import { rxResource } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Pencil, Plus } from 'lucide-angular';

@Component({
  selector: 'app-my-submissions-page',
  imports: [CommonModule, RouterLink, LucideAngularModule],
  templateUrl: './my-submissions-page.component.html',
})
export class MySubmissionsPageComponent {
  private booksService = inject(BooksService);
  private router = inject(Router);

  BookSubmissionStatusEnum = BookSubmissionStatus

  // Resource para cargar las submissions
  submissionsResource = rxResource({
    loader: () => this.booksService.getMySubmissions()
  });

  // Señal computada para las submissions
  submissions = this.submissionsResource.value;

  // Contadores por estado
  pendingCount = () => this.submissions()!.filter(s => s.status === BookSubmissionStatus.Pending).length;
  approvedCount = () => this.submissions()!.filter(s => s.status === BookSubmissionStatus.Approved).length;
  rejectedCount = () => this.submissions()!.filter(s => s.status === BookSubmissionStatus.Rejected).length;

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

  // Navegar a los detalles de una submission
  viewSubmission(id: number) {
    this.router.navigate(['/write/submissions', id]);
  }

  // Eliminar una submission
  async deleteSubmission(id: number, title: string) {
    if (!confirm(`¿Estás seguro de que quieres cancelar la solicitud "${title}"?`)) {
      return;
    }

    try {
      await this.booksService.deleteSubmission(id).toPromise();
      this.submissionsResource.reload(); // Recargar la lista
    } catch (error) {
      console.error('Error eliminando submission:', error);
      alert('Error al cancelar la solicitud');
    }
  }

  readonly plus = Plus;
  readonly edit = Pencil;
}
