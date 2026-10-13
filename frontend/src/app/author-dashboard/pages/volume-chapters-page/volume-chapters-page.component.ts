import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BooksService, ChapterAdminDto, ChapterDto, CreateChapterDto, UpdateChapterDto } from '../../../features/books/services/books.service';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MarkdownEditorComponent } from '../../../features/books/components/markdown-editor/markdown-editor.component';
import { ArrowLeft, Eye, EyeOff, LucideAngularModule, Pencil, Plus } from 'lucide-angular';

@Component({
  selector: 'app-volume-chapters-page',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, MarkdownEditorComponent, LucideAngularModule],
  templateUrl: './volume-chapters-page.component.html',
})
export class VolumeChaptersPageComponent {
  private booksService = inject(BooksService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  bookId = this.route.snapshot.params['bookId'];
  volumeId = this.route.snapshot.params['volumeId'];

  isCreating = signal(false);
  isEditing = signal<number | null>(null);
  selectedChapter = signal<ChapterAdminDto | null>(null);
  isToggling = signal<number | null>(null);
  isLoadingChapter = signal(false);

  // Resource para cargar capítulos
  chaptersResource = rxResource({
    request: () => ({ volumeId: this.volumeId }),
    loader: ({ request }) => this.booksService.getChaptersByVolume(request.volumeId)
  });

  // Formularios
  createChapterForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(1)]],
    content: ['', [Validators.required, Validators.minLength(500), Validators.maxLength(20000)]],
    order: [1, [Validators.required, Validators.min(1)]]
  });

  editChapterForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(1)]],
    content: ['', [Validators.required, Validators.minLength(500), Validators.maxLength(20000)]],
    order: [1, [Validators.required, Validators.min(1)]]
  });

  // Métodos para crear capítulo - ACTUALIZADO
  startCreating() {
    this.isCreating.set(true);
    this.createChapterForm.reset({
      title: '',
      content: '',
      order: 1
    });
  }

  cancelCreating() {
    this.isCreating.set(false);
  }

  async createChapter() {
    if (this.createChapterForm.invalid) return;

    try {
      const chapterData: CreateChapterDto = {
        title: this.createChapterForm.get('title')?.value,
        content: this.createChapterForm.get('content')?.value,
        volumeId: Number(this.volumeId),
        order: Number(this.createChapterForm.get('order')?.value) || 1
      };

      console.log('Datos enviados:', chapterData);
      await this.booksService.createChapter(chapterData).toPromise();
      this.chaptersResource.reload();
      this.isCreating.set(false);
      this.createChapterForm.reset({ order: 1 });
    } catch (error) {
      console.error('Error creando capítulo:', error);
      alert('Error al crear el capítulo');
    }
  }

  // Métodos para editar capítulo - ACTUALIZADO
  async startEditing(chapter: ChapterDto) {
    this.isEditing.set(chapter.id);
    this.isLoadingChapter.set(true);

    try {
      // Cargar el capítulo completo con contenido usando getChapterForAdmin
      const fullChapter = await this.booksService.getChapterForAdmin(chapter.id).toPromise();
      this.selectedChapter.set(fullChapter!);

      // Llenar el formulario con los datos completos
      this.editChapterForm.patchValue({
        title: fullChapter!.title,
        content: fullChapter!.content || '',
        order: fullChapter!.order // Incluir el orden actual
      });
    } catch (error) {
      console.error('Error cargando capítulo para editar:', error);
      alert('Error al cargar el capítulo para editar');
      this.cancelEditing();
    } finally {
      this.isLoadingChapter.set(false);
    }
  }

  cancelEditing() {
    this.isEditing.set(null);
    this.selectedChapter.set(null);
    this.isLoadingChapter.set(false);
  }

  async updateChapter() {
    if (this.editChapterForm.invalid || !this.selectedChapter()) return;

    try {
      const chapterData: UpdateChapterDto = {
        title: this.editChapterForm.get('title')?.value,
        content: this.editChapterForm.get('content')?.value,
        order: Number(this.editChapterForm.get('order')?.value) // Incluir el nuevo orden
      };

      console.log('Datos de actualización:', chapterData);
      await this.booksService.updateChapter(this.selectedChapter()!.id, chapterData).toPromise();
      this.chaptersResource.reload();
      this.cancelEditing();
    } catch (error) {
      console.error('Error actualizando capítulo:', error);
      alert('Error al actualizar el capítulo');
    }
  }

  // Resto de métodos se mantienen igual...
  async toggleChapterActive(chapter: ChapterDto) {
    const newStatus = !chapter.isActive;
    const action = newStatus ? 'activar' : 'desactivar';

    if (!confirm(`¿Estás seguro de que quieres ${action} el capítulo "${chapter.title}"?`)) {
      return;
    }

    this.isToggling.set(chapter.id);

    try {
      await this.booksService.toggleChapterActive(chapter.id, newStatus).toPromise();
      this.chaptersResource.reload();
    } catch (error) {
      console.error('Error cambiando estado del capítulo:', error);
      alert('Error al cambiar el estado del capítulo');
    } finally {
      this.isToggling.set(null);
    }
  }

  goBackToVolumes() {
    this.router.navigate(['/write/books', this.bookId, 'volumes']);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('es-ES');
  }

  onCreateChapterContentChange(content: string) {
    this.createChapterForm.get('content')?.setValue(content);
    this.createChapterForm.get('content')?.markAsTouched();
  }

  onEditChapterContentChange(content: string) {
    this.editChapterForm.get('content')?.setValue(content);
    this.editChapterForm.get('content')?.markAsTouched();
  }

  isContentInvalid(form: FormGroup): boolean {
    const control = form.get('content');
    return control ? (control.invalid && control.touched) : false;
  }

  getContentError(form: FormGroup): string {
    const control = form.get('content');
    if (!control || !control.errors) return '';

    if (control.errors['required']) return 'El contenido del capítulo es requerido';
    if (control.errors['minlength']) return 'El contenido debe tener al menos 500 caracteres';
    if (control.errors['maxlength']) return 'El contenido no puede exceder los 20,000 caracteres';

    return 'Contenido inválido';
  }

  readonly arrowLeft = ArrowLeft;
  readonly plus = Plus;
  // readonly chapters = ListOrdered;
  readonly edit = Pencil;
  readonly active = Eye;
  readonly desactive = EyeOff;

}
