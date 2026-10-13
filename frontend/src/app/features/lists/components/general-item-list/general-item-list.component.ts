import { Component, input, output, signal, inject } from '@angular/core';
import { List } from '../../interfaces/list-response.interface';
import { Calendar, Edit, Globe, Heart, ListOrdered, Lock, LucideAngularModule, Trash2, User, Users } from 'lucide-angular';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time.pipe';
import { RouterLink } from '@angular/router';
import { ListsService } from '../../services/lists.service';
import { NgIf } from '@angular/common';

@Component({
  selector: 'general-item-list',
  imports: [LucideAngularModule, RelativeTimePipe, RouterLink, NgIf],
  templateUrl: './general-item-list.component.html',
})
export class GeneralItemListComponent {
   private listsService = inject(ListsService);

  list = input<List|null>();
  showActions = input(false);
  showFollowButton = input(false); // Nuevo input para mostrar botón de seguir/dejar de seguir

  // Outputs
  listDeleted = output<number>();
  listUnfollowed = output<number>(); // Nuevo output para dejar de seguir

  // Estados para el modal de eliminación
  showDeleteModal = signal(false);
  isDeleting = signal(false);
  deleteError = signal<string | null>(null);

  // Estados para seguir/dejar de seguir
  isFollowingLoading = signal(false);
  followError = signal<string | null>(null);

  // Iconos
  readonly user = User;
  readonly cant = ListOrdered;
  readonly calendar = Calendar;
  readonly follow = Users;
  readonly edit = Edit;
  readonly deleteIcon = Trash2;
  readonly lockIcon = Lock;
  readonly globeIcon = Globe;
  readonly heartIcon = Heart;

  imageError = signal(false);

  onImageError() {
    this.imageError.set(true);
  }

  // Abrir modal de confirmación
  confirmDelete(): void {
    this.showDeleteModal.set(true);
    this.deleteError.set(null);
  }

  // Cancelar eliminación
  cancelDelete(): void {
    this.showDeleteModal.set(false);
    this.deleteError.set(null);
    this.isDeleting.set(false);
  }

  // Ejecutar eliminación
  onDelete(): void {
    const listId = this.list()?.id;
    if (!listId) return;

    this.isDeleting.set(true);
    this.deleteError.set(null);

    this.listsService.deleteList(listId).subscribe({
      next: (response) => {
        this.isDeleting.set(false);
        this.showDeleteModal.set(false);

        if (response.success) {
          this.listDeleted.emit(listId);
        } else {
          this.deleteError.set(response.errors?.[0] || 'Error al eliminar la lista');
        }
      },
      error: (error) => {
        this.isDeleting.set(false);
        this.deleteError.set(error.errors?.[0] || 'Error inesperado al eliminar la lista');
      }
    });
  }

  // Dejar de seguir lista
  async unfollowList(): Promise<void> {
    const listId = this.list()?.id;
    if (!listId) return;

    this.isFollowingLoading.set(true);
    this.followError.set(null);

    try {
      await this.listsService.unfollowList(listId).toPromise();
      this.listUnfollowed.emit(listId);
    } catch (error: any) {
      console.error('Error dejando de seguir lista:', error);
      let errorMessage = 'Error al dejar de seguir la lista';

      if (error.status === 400) {
        errorMessage = error.error?.message || 'No se pudo dejar de seguir la lista';
      }

      this.followError.set(errorMessage);

      // Auto-ocultar error después de 3 segundos
      setTimeout(() => {
        this.followError.set(null);
      }, 3000);
    } finally {
      this.isFollowingLoading.set(false);
    }
  }


}
