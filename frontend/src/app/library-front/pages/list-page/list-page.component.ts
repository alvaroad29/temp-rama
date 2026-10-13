import { Component, computed, inject, signal } from '@angular/core';
import { ListsService } from '../../../features/lists/services/lists.service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { rxResource } from '@angular/core/rxjs-interop';
import { PaginationService } from '../../../shared/components/pagination/pagination.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { BookOpen, Calendar, Edit, Heart, ListOrdered, LucideAngularModule, Star, Trash2, User, Users } from 'lucide-angular';
import { RelativeTimePipe } from '../../../shared/pipes/relative-time.pipe';
import { CardData } from '../../../features/books/interfaces/card-data.interface';
import { State } from '../../../features/books/interfaces/book-response.interface';
import { BookList } from '../../../features/lists/interfaces/list-detailed.interface';
import { BookCardComponent } from '../../../features/books/components/book-card/book-card.component';
import { AuthService } from '../../../auth/services/auth.service';
import { NgClass } from '@angular/common';

interface Notification {
  id: number;
  message: string;
  type: 'success' | 'error';
  visible: boolean;
}
@Component({
  selector: 'app-list-page',
  imports: [PaginationComponent, LucideAngularModule, RelativeTimePipe, BookCardComponent, NgClass, RouterLink],
  templateUrl: './list-page.component.html',
})
export class ListPageComponent {
   activatedRoute = inject(ActivatedRoute);
  paginationService = inject(PaginationService);
  private listsService = inject(ListsService);
  private authService = inject(AuthService);

  listId: number = this.activatedRoute.snapshot.params['listId'];

  // Señales para notificaciones
  notifications = signal<Notification[]>([]);
  private notificationId = 0;

  // Señal para estado de seguimiento
  isFollowing = signal<boolean>(false);
  isFollowingLoading = signal<boolean>(false);

  listResource = rxResource({
    request: () => ({
      page: this.paginationService.currentPage(),
      id: this.listId
    }),
    loader: ({ request }) => {
      return this.listsService.getDetailedList(request.id, {
        PageNumber: request.page
      });
    },
  });

  // Iconos
  readonly userIcon = User;
  readonly calendarIcon = Calendar;
  readonly followersIcon = Users;
  readonly bookIcon = ListOrdered;
  readonly bookOpenIcon = BookOpen;
  readonly starIcon = Star;
  readonly trashIcon = Trash2;
  readonly heartIcon = Heart;
  readonly editIcon = Edit;

  listData = computed(() => {
    const result = this.listResource.value();
    return result || null;
  });

  books = computed(() => {
    const data = this.listData()?.books?.data;
    if (!data) return [];
    return this.mapBookToCardData(data);
  });

  totalPages = computed(() => {
    return this.listData()?.books?.totalPages || 0;
  });

  isLoading = computed(() => this.listResource.isLoading());
  error = computed(() => this.listResource.error());

  // Verificar si el usuario actual es el dueño de la lista
  isOwner = computed(() => {
    const currentUser = this.authService.user();
    const list = this.listData();
    return currentUser && list && currentUser.id === list.creatorId;
  });

  // Verificar si el usuario está autenticado
  isAuthenticated = computed(() => {
    return this.authService.isAuthenticated();
  });

  ngOnInit() {
    // Verificar si el usuario sigue la lista cuando se carga el componente
    this.checkIfFollowing();
  }

  private mapBookToCardData(books: BookList[]): CardData[] {
    return books.map(book => ({
      title: book.title,
      status: this.getStatusText(book.state),
      image: book.imageUrl || './assets/book-placeholder.png',
      score: book.score,
      id: book.id.toString()
    }));
  }

  // Función para obtener el texto del estado
  getStatusText(state: State): string {
    switch (state) {
      case State.Activo:
        return 'Activo';
      case State.Completado:
        return 'Completado';
      case State.Pausado:
        return 'Pausado';
      case State.All:
        return 'Todos';
      default:
        return 'Desconocido';
    }
  }

  // Método para verificar si el usuario sigue la lista
  async checkIfFollowing() {
    if (!this.isAuthenticated()) return;

    try {
      this.isFollowingLoading.set(true);
      const isFollowing = await this.listsService.checkIfFollowing(this.listId).toPromise();
      this.isFollowing.set(isFollowing || false);
    } catch (error) {
      console.error('Error verificando seguimiento:', error);
    } finally {
      this.isFollowingLoading.set(false);
    }
  }

  // Método para seguir la lista
  async followList() {
    if (!this.isAuthenticated()) {
      this.showNotification('Debes iniciar sesión para seguir listas', 'error');
      return;
    }

    try {
      this.isFollowingLoading.set(true);
      await this.listsService.followList(this.listId).toPromise();
      this.isFollowing.set(true);
      this.showNotification('Lista seguida correctamente', 'success');

      // Recargar los datos para actualizar el contador de seguidores
      this.listResource.reload();
    } catch (error: any) {
      console.error('Error siguiendo lista:', error);
      let errorMessage = 'Error al seguir la lista';

      if (error.status === 400) {
        errorMessage = error.error?.message || 'No se pudo seguir la lista';
      }

      this.showNotification(errorMessage, 'error');
    } finally {
      this.isFollowingLoading.set(false);
    }
  }

  // Método para dejar de seguir la lista
  async unfollowList() {
    try {
      this.isFollowingLoading.set(true);
      await this.listsService.unfollowList(this.listId).toPromise();
      this.isFollowing.set(false);
      this.showNotification('Lista dejada de seguir', 'success');

      // Recargar los datos para actualizar el contador de seguidores
      this.listResource.reload();
    } catch (error: any) {
      console.error('Error dejando de seguir lista:', error);
      let errorMessage = 'Error al dejar de seguir la lista';

      if (error.status === 400) {
        errorMessage = error.error?.message || 'No se pudo dejar de seguir la lista';
      }

      this.showNotification(errorMessage, 'error');
    } finally {
      this.isFollowingLoading.set(false);
    }
  }

  // Método para eliminar un libro de la lista
  async removeBookFromList(bookId: number, bookTitle: string) {
    if (!confirm(`¿Estás seguro de que quieres eliminar "${bookTitle}" de esta lista?`)) {
      return;
    }

    try {
      await this.listsService.removeBookFromList(this.listId, bookId).toPromise();
      this.showNotification(`"${bookTitle}" eliminado de la lista`, 'success');
      this.listResource.reload();
    } catch (error: any) {
      console.error('Error eliminando libro de la lista:', error);
      let errorMessage = 'Error al eliminar el libro de la lista';

      if (error.status === 403) {
        errorMessage = 'No tienes permisos para eliminar libros de esta lista';
      } else if (error.status === 404) {
        errorMessage = 'El libro no fue encontrado en la lista';
      }

      this.showNotification(errorMessage, 'error');
    }
  }

  // Métodos para notificaciones
  showNotification(message: string, type: 'success' | 'error') {
    const id = ++this.notificationId;
    const notification: Notification = {
      id,
      message,
      type,
      visible: true
    };

    this.notifications.update(notifications => [...notifications, notification]);

    setTimeout(() => {
      this.hideNotification(id);
    }, 2000);

    setTimeout(() => {
      this.removeNotification(id);
    }, 2500);
  }

  hideNotification(id: number) {
    this.notifications.update(notifications =>
      notifications.map(notification =>
        notification.id === id ? { ...notification, visible: false } : notification
      )
    );
  }

  removeNotification(id: number) {
    this.notifications.update(notifications =>
      notifications.filter(notification => notification.id !== id)
    );
  }

  closeNotification(id: number) {
    this.hideNotification(id);
    setTimeout(() => this.removeNotification(id), 300);
  }
}
