import { Component, inject, input, output, signal } from '@angular/core';
import { AuthService } from '../../../../auth/services/auth.service';
import { ListsService } from '../../services/lists.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { BookList, BooksListResponse } from '../../interfaces/list-detailed.interface';
import { of } from 'rxjs';
import { Router, RouterLink } from '@angular/router';
import { CommonModule, NgClass } from '@angular/common';
import { BookListWithMembershipDto } from '../../interfaces/book-list-membership';

interface Notification {
  id: number;
  message: string;
  type: 'success' | 'error' | 'warning';
  visible: boolean;
}

@Component({
  selector: 'add-to-list',
  imports: [CommonModule, RouterLink, NgClass],
  templateUrl: './add-to-list.component.html',
})
export class AddToListComponent {
  bookId = input.required<number>();



  private authService = inject(AuthService);
  private listsService = inject(ListsService);
  router = inject(Router);

  authStatus = this.authService.authStatus;
  isModalOpen = signal(false);

  // Señal para manejar las notificaciones
  notifications = signal<Notification[]>([]);
  private notificationId = 0;

  // RXResource para cargar las listas del usuario
  listsResource = rxResource({
    request: () => ({
      userId: this.authService.user()?.id,
      bookId: this.bookId()
    }),
    loader: ({ request }) => {
      if (!request.userId) {
        throw new Error('Usuario no autenticado');
      }
      return this.listsService.getUserBookListsWithBookMembership(request.userId, request.bookId);
    },
  });

  openModal() {
    this.isModalOpen.set(true);
    // Recargar las listas cuando se abre el modal
    this.listsResource.reload();
  }

  closeModal() {
    this.isModalOpen.set(false);
  }

  // Método para mostrar notificaciones
  showNotification(message: string, type: 'success' | 'error' | 'warning') {
    const id = ++this.notificationId;
    const notification: Notification = {
      id,
      message,
      type,
      visible: true
    };

    // Agregar la notificación
    this.notifications.update(notifications => [...notifications, notification]);

    // Ocultar después de 4 segundos
    setTimeout(() => {
      this.hideNotification(id);
    }, 4000);

    // Eliminar completamente después de la animación
    setTimeout(() => {
      this.removeNotification(id);
    }, 4500);
  }

  // Ocultar notificación con animación
  hideNotification(id: number) {
    this.notifications.update(notifications =>
      notifications.map(notification =>
        notification.id === id ? { ...notification, visible: false } : notification
      )
    );
  }

  // Eliminar notificación del array
  removeNotification(id: number) {
    this.notifications.update(notifications =>
      notifications.filter(notification => notification.id !== id)
    );
  }

  // Cerrar notificación manualmente
  closeNotification(id: number) {
    this.hideNotification(id);
    setTimeout(() => this.removeNotification(id), 300);
  }

  async onListToggle(list: BookListWithMembershipDto, event: Event) {
    const checked = (event.target as HTMLInputElement).checked;

    try {
      if (checked) {
        // Agregar libro a la lista
        await this.listsService.addBookToList(list.id, this.bookId()).toPromise();
        // Actualizar el estado local
        list.containsBook = true;
        list.bookCount++;
        this.showNotification(`Libro agregado a "${list.title}"`, 'success');
      } else {
        // Remover libro de la lista
        await this.listsService.removeBookFromList(list.id, this.bookId()).toPromise();
        // Actualizar el estado local
        list.containsBook = false;
        list.bookCount = Math.max(0, list.bookCount - 1);
        this.showNotification(`Libro eliminado de "${list.title}"`, 'success');
      }
    } catch (error: any) {
      console.error('Error al actualizar la lista:', error);

      // Revertir el cambio en caso de error
      (event.target as HTMLInputElement).checked = !checked;

      // Manejar diferentes tipos de errores
      if (error.status === 400) {
        const errorMessage = error.error?.message || 'Error al procesar la solicitud';

        if (errorMessage.includes('límite máximo') || errorMessage.includes('100')) {
          this.showNotification(`La lista "${list.title}" ha alcanzado el límite máximo de 100 libros`, 'warning');
        } else if (errorMessage.includes('ya existe')) {
          this.showNotification(`El libro ya existe en "${list.title}"`, 'warning');
        } else {
          this.showNotification(errorMessage, 'error');
        }
      } else if (error.status === 403) {
        this.showNotification('No tienes permisos para modificar esta lista', 'error');
      } else if (error.status === 404) {
        this.showNotification('La lista no fue encontrada', 'error');
      } else {
        this.showNotification('Error de conexión. Intenta nuevamente', 'error');
      }
    }
  }
}
