import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import { ListsService } from '../../../features/lists/services/lists.service';
import { AuthService } from '../../../auth/services/auth.service';
import { GeneralItemListComponent } from '../../../features/lists/components/general-item-list/general-item-list.component';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'lists-page',
  imports: [DatePipe, GeneralItemListComponent, RouterLink],
  templateUrl: './lists-page.component.html',
})
export class ListsPageComponent {
  // HACER: mostrar mensaje y deasctivar boton si ya alcansaste las 20 listas creadas
  private authService = inject(AuthService);
  private listsService = inject(ListsService);

  user = computed(() => this.authService.user());

  // Señal para mensajes de éxito
  successMessage = signal<string | null>(null);

  listsResource = rxResource({
    request: () => ({
      id: this.user()!.id
    }),
    loader: ({request}) => {
        return this.listsService.getListsByUser(request.id);
      },
    }
  );

  lists = computed(() => {
    const result = this.listsResource.value();
    return result || [];
  });

  isLoading = computed(() => this.listsResource.isLoading());
  error = computed(() => this.listsResource.error());

  onListDeleted(listId: number): void {
    // Recargar las listas
    this.listsResource.reload();

    // Mostrar mensaje de éxito
    this.successMessage.set('Lista eliminada correctamente');

    // Limpiar mensaje después de 3 segundos
    setTimeout(() => {
      this.successMessage.set(null);
    }, 3000);
  }
}
