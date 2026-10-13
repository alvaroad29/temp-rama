import { Component, computed, ElementRef, inject, ViewChild } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time.pipe';
import { AnnoucementsService, ApiResponse } from '../../../../shared/services/annoucements.service';
import { rxResource } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-annoucement-list-page',
  imports: [RelativeTimePipe, RouterLink],
  templateUrl: './annoucement-list-page.component.html',
  styles: [`
    .line-clamp-2 {
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .line-clamp-3 {
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
  `]
})
export class AnnoucementListPageComponent {

  private annoucementService = inject(AnnoucementsService);

  annoucementResource = rxResource({
    loader: () => {
        return this.annoucementService.getAnnoucements();
      },
    });

  data = computed(() => {
    const resource = this.annoucementResource.value();
    return resource || [];
  });

  router = inject(Router);



  showNotification = false;
  notificationMessage = '';
  notificationType: 'success' | 'error' = 'success';

  private showTempNotification(message: string, type: 'success' | 'error', duration: number = 3000): void {
    this.notificationMessage = message;
    this.notificationType = type;
    this.showNotification = true;

    setTimeout(() => {
      this.showNotification = false;
    }, duration);
  }

    @ViewChild('deleteModal') deleteModal!: ElementRef;
  announcementToDelete: number | null = null; // <- Esta variable guarda el ID

  openDeleteModal(id: number): void {
    this.announcementToDelete = id; // Guardar el ID seleccionado
    this.deleteModal.nativeElement.showModal();
  }

  closeDeleteModal(): void {
    this.announcementToDelete = null; // Limpiar el ID
    this.deleteModal.nativeElement.close();
  }

  deleteAnnouncement(): void { // <- Quitar el parámetro id
    if (this.announcementToDelete) {
      console.log({id: this.announcementToDelete}); // Ahora sí será el correcto

      this.annoucementService.deleteAnnoucement(this.announcementToDelete).subscribe({
        next: (response) => {
          if (response.success) {
            this.showTempNotification('Anuncio eliminado correctamente', 'success');
            this.annoucementResource.reload();
          } else {
            this.showTempNotification(
              response.errors?.join(', ') || 'Error desconocido',
              'error'
            );
          }
        },
        error: (errorResponse: ApiResponse) => {
          this.showTempNotification(
            errorResponse.errors?.join('\n') || 'Error al eliminar',
            'error'
          );
        }
      });
      this.closeDeleteModal();
    }
  }

}
