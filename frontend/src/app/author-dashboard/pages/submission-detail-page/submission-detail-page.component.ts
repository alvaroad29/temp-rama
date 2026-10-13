import { Component, computed, inject, input, signal } from '@angular/core';
import { BooksService, BookSubmissionStatus, UpdateBookSubmissionStatusDto } from '../../../features/books/services/books.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { rxResource } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { MarkdownModule } from 'ngx-markdown';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, ArrowLeft } from 'lucide-angular';

@Component({
  selector: 'app-submission-detail-page',
  imports: [CommonModule, RouterLink, MarkdownModule, FormsModule, LucideAngularModule],
  templateUrl: './submission-detail-page.component.html',
    styles: [`
    .markdown-content {
      max-height: 600px;
      overflow-y: auto;
    }

    .markdown-content ::ng-deep {
      h1, h2, h3, h4, h5, h6 {
        margin-top: 1.5em;
        margin-bottom: 0.5em;
      }

      p {
        margin-bottom: 1em;
        line-height: 1.6;
      }

      ul, ol {
        margin-bottom: 1em;
        padding-left: 2em;
      }

      code {
        background: rgba(148, 163, 184, 0.1);
        padding: 0.2em 0.4em;
        border-radius: 0.25rem;
        font-size: 0.875em;
      }

      pre {
        background: rgba(148, 163, 184, 0.1);
        padding: 1em;
        border-radius: 0.5rem;
        overflow-x: auto;
        margin-bottom: 1em;
      }

      blockquote {
        border-left: 4px solid #94a3b8;
        padding-left: 1em;
        margin-left: 0;
        color: #64748b;
        font-style: italic;
      }
    }
  `]

})
export class SubmissionDetailPageComponent {
   private booksService = inject(BooksService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // Cambia el input por una señal regular
  context = signal<'author' | 'admin'>('author');

  // Exponer el enum al template
  BookSubmissionStatusEnum = BookSubmissionStatus;

  // Para el modo admin - usar propiedades normales en lugar de señales para ngModel
  submissionDecision: BookSubmissionStatus | null = null;
  submissionFeedback = '';

  // Señal para el estado de actualización
  isUpdating = signal(false);

  // Obtener el ID de la ruta
  submissionId = this.route.snapshot.params['id'];

  // Resource para cargar la submission específica
  submissionResource = rxResource({
    request: () => ({ id: this.submissionId }),
    loader: ({ request }) => this.booksService.getSubmission(request.id)
  });

  genresResource = rxResource({
    loader: () => this.booksService.getGenres(),
  });
  genres = computed(() => this.genresResource.value() ?? []);

submissionGenres = computed(() => {
  if (!this.submissionResource.value()?.genreIds || !this.genres().length) {
    return [];
  }

  const genreMap = new Map(this.genres().map(genre => [genre.id, genre]));

  return this.submissionResource.value()?.genreIds
    .map(id => {
      const genre = genreMap.get(id);
      return genre ? genre : { id, name: 'Género no encontrado', isActive: false };
    })
    .filter(genre => genre !== null);
});

  ngOnInit() {
    // Leer el contexto de los datos de la ruta
    this.route.data.subscribe(data => {
      if (data['context']) {
        this.context.set(data['context']);
      }
    });
  }

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
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // Métodos específicos para autor
  async deleteSubmission(id: number, title: string) {
    if (!confirm(`¿Estás seguro de que quieres cancelar la solicitud "${title}"?`)) {
      return;
    }

    try {
      await this.booksService.deleteSubmission(id).toPromise();
      alert('Solicitud cancelada correctamente');
      this.navigateBack();
    } catch (error) {
      console.error('Error eliminando submission:', error);
      alert('Error al cancelar la solicitud');
    }
  }

  // Métodos específicos para admin
  async updateSubmissionStatus() {
    const submission = this.submissionResource.value();

    if (!submission || !this.submissionDecision) {
      alert('Por favor selecciona una decisión');
      return;
    }

    this.isUpdating.set(true);

    try {
      const updateDto: UpdateBookSubmissionStatusDto = {
        status: this.submissionDecision,
        adminFeedback: this.submissionFeedback
      };

      await this.booksService.updateSubmissionStatus(submission.id, updateDto).toPromise();

      // Recargar la submission
      this.submissionResource.reload();

      // Limpiar el formulario
      this.submissionDecision = null;
      this.submissionFeedback = '';

      alert(`Solicitud ${this.getStatusText(this.submissionDecision!).toLowerCase()} correctamente`);

    } catch (error) {
      console.error('Error actualizando submission:', error);
      alert('Error al procesar la solicitud');
    } finally {
      this.isUpdating.set(false);
    }
  }

  // Navegación según el contexto
  navigateBack() {
    if (this.context() === 'admin') {
      this.router.navigate(['/admin/submissions']);
    } else {
      this.router.navigate(['/write/submissions']);
    }
  }

  // Helper para verificar si es admin
  get isAdminContext(): boolean {
    return this.context() === 'admin';
  }

  // Helper para verificar si el admin puede editar (solo pendientes)
  get canAdminEdit(): boolean {
    const submission = this.submissionResource.value();
    return this.isAdminContext && submission?.status === BookSubmissionStatus.Pending;
  }

  readonly arrowLeft = ArrowLeft;
}
