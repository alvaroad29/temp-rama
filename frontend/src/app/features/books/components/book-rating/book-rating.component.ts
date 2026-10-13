import { Component, OnInit, signal, computed, inject, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BooksService } from '../../services/books.service';
import { AuthService } from '../../../../auth/services/auth.service';

@Component({
  selector: 'app-book-rating',
  imports: [CommonModule],
  templateUrl: './book-rating.component.html',
})
export class BookRatingComponent implements OnInit {
  bookId = input.required<number>();
  bookScore = input.required<number>();
  totalRatings = input.required<number>();

  ratingChanged = output<{newScore: number, newTotalRatings: number}>();


  private booksService = inject(BooksService);
  private authService = inject(AuthService);

  // Signals para el estado
  userRating = signal<number | null>(null);
  selectedRating = signal<number>(0);
  hoverRating = signal<number>(0);
  isSubmitting = signal<boolean>(false);
  isEditing = signal<boolean>(false);
  isDeleting = signal<boolean>(false);
  message = signal<{type: 'success' | 'error', text: string} | null>(null);


  // Computed para verificar autenticación
  isAuthenticated = computed(() => this.authService.isAuthenticated());

  // Referencia a Math para el template
  Math = Math;

  ngOnInit() {
    if (this.isAuthenticated()) {
      this.loadUserRating();
    }
  }

  private loadUserRating() {
    this.booksService.getUserRating(this.bookId()).subscribe({
      next: (response) => {
        this.userRating.set(response.rating);
      },
      error: (error) => {
        // console.error('Error loading user rating:', error);
      }
    });
  }

  selectRating(rating: number) {
    this.selectedRating.set(rating);
    this.clearMessage();
  }

  setHoverRating(rating: number) {
    this.hoverRating.set(rating);
  }

  submitRating() {
    if (this.selectedRating() === 0) return;

    this.isSubmitting.set(true);
    this.clearMessage();

    // Capturar valores para cálculo simple
    const currentScore = this.bookScore();
    const currentTotal = this.totalRatings();
    const wasFirstRating = !this.userRating();
    const selectedRating = this.selectedRating();

    this.booksService.rateBook(this.bookId(), selectedRating).subscribe({
      next: (response) => {
        this.userRating.set(response.ratingValue);
        this.isEditing.set(false);
        this.selectedRating.set(0);
        this.showMessage('success', '¡Calificación guardada exitosamente!');

        if (wasFirstRating) {
          // Primera calificación
          const newTotal = currentTotal + 1;
          const newScore = ((currentScore * currentTotal) + selectedRating) / newTotal;
          this.updateUI(newScore, newTotal);
        } else {
          // Actualizar calificación existente - mantener total igual
          // Para simplicidad, asumimos que el promedio cambió
          this.updateUI(currentScore, currentTotal);
        }
      },
      error: (error) => {
        this.showMessage('error', 'Error al guardar la calificación. Inténtalo de nuevo.');
      },
      complete: () => {
        this.isSubmitting.set(false);
      }
    });
  }

  deleteRating() {
    if (!this.userRating() || this.isDeleting()) return;

    // Capturar valores actuales
    const currentScore = this.bookScore();
    const currentTotal = this.totalRatings();
    const userRatingValue = this.userRating()!;

    this.isDeleting.set(true);
    this.clearMessage();

    this.booksService.deleteRating(this.bookId()).subscribe({
      next: (response) => {
        this.userRating.set(null);
        this.isEditing.set(false);
        this.selectedRating.set(0);
        this.hoverRating.set(0);
        this.showMessage('success', 'Calificación eliminada exitosamente');

        if (currentTotal <= 1) {
          // Era la única calificación
          this.updateUI(0, 0);
        } else {
          // Había más calificaciones
          const newTotal = currentTotal - 1;
          const newScore = ((currentScore * currentTotal) - userRatingValue) / newTotal;
          this.updateUI(newScore, newTotal);
        }
      },
      error: (error) => {
        console.error('Error deleting rating:', error);
        let errorMessage = 'Error al eliminar la calificación';

        if (error.status === 404) {
          errorMessage = 'No se encontró una calificación para eliminar';
        } else if (error.status === 401) {
          errorMessage = 'No tienes permisos para realizar esta acción';
        }

        this.showMessage('error', errorMessage);
      },
      complete: () => {
        this.isDeleting.set(false);
      }
    });
  }

  private updateUI(newScore: number, newTotal: number) {
    this.ratingChanged.emit({
      newScore: Math.round(newScore * 100) / 100,
      newTotalRatings: newTotal
    });
  }

  toggleEditMode() {
    this.isEditing.set(!this.isEditing());
    this.selectedRating.set(this.userRating() || 0);
    this.clearMessage();
  }

  cancelEdit() {
    this.isEditing.set(false);
    this.selectedRating.set(0);
    this.hoverRating.set(0);
    this.clearMessage();
  }

  private showMessage(type: 'success' | 'error', text: string) {
    this.message.set({ type, text });
    // Auto-ocultar mensaje después de 3 segundos
    setTimeout(() => this.clearMessage(), 3000);
  }

  private clearMessage() {
    this.message.set(null);
  }
}
