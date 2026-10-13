import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Edit, Trash2, Power, PowerOff } from 'lucide-angular';
import { Genre } from '../../../../features/books/interfaces/book-response.interface';
import { ApiResponse, BooksService } from '../../../../features/books/services/books.service';

interface Notification {
  id: number;
  message: string;
  type: 'success' | 'error';
}

@Component({
  selector: 'app-admin-genres-page',
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './admin-genres-page.component.html',
})
export class AdminGenresPageComponent {
  private genresService = inject(BooksService);

  genres = signal<Genre[]>([]);
  isLoading = signal(true);
  loadingActions = signal<{ [key: number]: boolean }>({});

  // Sistema de notificaciones
  private notificationId = 0;
  notifications = signal<Notification[]>([]);

  // Iconos
  readonly editIcon = Edit;
  readonly deleteIcon = Trash2;
  readonly powerOnIcon = Power;
  readonly powerOffIcon = PowerOff;

  ngOnInit() {
    this.loadGenres();
  }

  loadGenres() {
    this.isLoading.set(true);
    this.genresService.getAllGenresForAdmin().subscribe({
      next: (genres) => {
        this.genres.set(genres);
        this.isLoading.set(false);
      },
      error: () => {
        this.showNotification('Error al cargar los géneros', 'error');
        this.isLoading.set(false);
      }
    });
  }

  toggleGenreActive(genre: Genre) {
    this.setLoading(genre.id, true);

    this.genresService.toggleGenreActive(genre.id).subscribe({
      next: (response: ApiResponse<{ currentStatus: boolean }>) => {
        if (response.success && response.data) {
          const updatedGenres = this.genres().map(g =>
            g.id === genre.id ? { ...g, isActive: response.data!.currentStatus } : g
          );
          this.genres.set(updatedGenres);

          const msg = `Género ${response.data.currentStatus ? 'activado' : 'desactivado'} correctamente`;
          this.showNotification(msg, 'success');
        } else {
          this.showNotification(response.errors?.[0] || 'Error al cambiar el estado del género', 'error');
        }
        this.setLoading(genre.id, false);
      },
      error: () => {
        this.showNotification('Error al cambiar el estado del género', 'error');
        this.setLoading(genre.id, false);
      }
    });
  }

  deleteGenre(genre: Genre) {
    if (!confirm(`¿Estás seguro de que quieres ${genre.isActive ? 'desactivar' : 'activar'} el género "${genre.name}"?`)) {
      return;
    }
    this.toggleGenreActive(genre);
  }

  private setLoading(genreId: number, loading: boolean) {
    this.loadingActions.update(actions => ({ ...actions, [genreId]: loading }));
  }

  isGenreLoading(genreId: number): boolean {
    return this.loadingActions()[genreId] || false;
  }

  // NUEVO: sistema de notificaciones
  private showNotification(message: string, type: 'success' | 'error') {
    const id = this.notificationId++;
    const notification: Notification = { id, message, type };
    this.notifications.update(notifs => [...notifs, notification]);

    // Auto-eliminar después de 2 segundos
    setTimeout(() => {
      this.dismissNotification(id);
    }, 2000);
  }

  private dismissNotification(id: number) {
    this.notifications.update(notifs => notifs.filter(n => n.id !== id));
  }

  // Computadas
  activeGenresCount = computed(() => this.genres().filter(g => g.isActive).length);
  inactiveGenresCount = computed(() => this.genres().filter(g => !g.isActive).length);
}
