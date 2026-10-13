import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BooksService, VolumeCreateDto, VolumeSimpleDto, VolumeUpdateDto } from '../../../features/books/services/books.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { ArrowLeft, Eye, EyeOff, ListOrdered, LucideAngularModule, Pencil, Plus } from 'lucide-angular';

@Component({
  selector: 'app-book-volumes-page',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, LucideAngularModule],
  templateUrl: './book-volumes-page.component.html',
})
export class BookVolumesPageComponent {
  private booksService = inject(BooksService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  bookId = this.route.snapshot.params['id'];
  isCreating = signal(false);
  isEditing = signal<number | null>(null);
  selectedVolume = signal<VolumeSimpleDto | null>(null);
  isToggling = signal<number | null>(null); // Nueva señal para el estado de toggle

  // Resource para cargar volúmenes
  volumesResource = rxResource({
    request: () => ({ bookId: this.bookId }),
    loader: ({ request }) => this.booksService.getVolumesByBook(request.bookId)
  });

  // Formularios
  createVolumeForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(1)]],
    order: [1, [Validators.required, Validators.min(1)]]
  });

  editVolumeForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(1)]],
    order: [1, [Validators.required, Validators.min(1)]]
  });

  // Métodos para crear volumen
  startCreating() {
    this.isCreating.set(true);
    this.createVolumeForm.reset({ title: '', order: 1 });
  }

  cancelCreating() {
    this.isCreating.set(false);
  }

  async createVolume() {
    if (this.createVolumeForm.invalid) return;

    try {
      const volumeData: VolumeCreateDto = {
        title: this.createVolumeForm.get('title')?.value,
        order: this.createVolumeForm.get('order')?.value,
        bookId: Number(this.bookId)
      };

      await this.booksService.createVolume(volumeData).toPromise();
      this.volumesResource.reload();
      this.isCreating.set(false);
      this.createVolumeForm.reset();
    } catch (error) {
      // console.error('Error creando volumen:', error);
      alert('Error al crear el volumen');
    }
  }

  // Métodos para editar volumen
  startEditing(volume: VolumeSimpleDto) {
    this.isEditing.set(volume.id);
    this.selectedVolume.set(volume);
    this.editVolumeForm.patchValue({
      title: volume.title,
      order: volume.order
    });
  }

  cancelEditing() {
    this.isEditing.set(null);
    this.selectedVolume.set(null);
  }

  async updateVolume() {
    if (this.editVolumeForm.invalid || !this.selectedVolume()) return;

    try {
      const volumeData: VolumeUpdateDto = {
        title: this.editVolumeForm.get('title')?.value,
        order: this.editVolumeForm.get('order')?.value
      };

      await this.booksService.updateVolume(this.selectedVolume()!.id, volumeData).toPromise();
      this.volumesResource.reload();
      this.cancelEditing();
    } catch (error) {
      console.error('Error actualizando volumen:', error);
      alert('Error al actualizar el volumen');
    }
  }

  // Método para activar/desactivar volumen
  async toggleVolumeActive(volume: VolumeSimpleDto) {
    const newStatus = !volume.isActive;
    const action = newStatus ? 'activar' : 'desactivar';

    if (!confirm(`¿Estás seguro de que quieres ${action} el volumen "${volume.title}"?`)) {
      return;
    }

    this.isToggling.set(volume.id);

    try {
      await this.booksService.toggleVolumeActive(volume.id, newStatus).toPromise();
      this.volumesResource.reload();
    } catch (error) {
      console.error('Error cambiando estado del volumen:', error);
      alert('Error al cambiar el estado del volumen');
    } finally {
      this.isToggling.set(null);
    }
  }

  // Navegar a capítulos
  viewChapters(volumeId: number) {
    this.router.navigate(['/write/books', this.bookId, 'volumes', volumeId, 'chapters']);
  }


  readonly arrowLeft = ArrowLeft;
  readonly plus = Plus;
  readonly chapters = ListOrdered;
  readonly edit = Pencil;
  readonly active = Eye;
  readonly desactive = EyeOff;


}
