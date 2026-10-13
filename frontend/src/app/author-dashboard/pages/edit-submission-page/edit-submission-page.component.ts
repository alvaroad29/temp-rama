import { Component, computed, effect, inject, signal } from '@angular/core';
import { MarkdownEditorComponent } from '../../../features/books/components/markdown-editor/markdown-editor.component';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BooksService, UpdateBookSubmissionDto } from '../../../features/books/services/books.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormUtils } from '../../../auth/utils/form-utils';

@Component({
  selector: 'app-edit-submission-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MarkdownEditorComponent],
  templateUrl: './edit-submission-page.component.html',
})
export class EditSubmissionPageComponent {
 private fb = inject(FormBuilder);
  private booksService = inject(BooksService);
  private route = inject(ActivatedRoute);
  router = inject(Router);
  formUtils = FormUtils;

  // Resource para cargar géneros
  genresResource = rxResource({
    loader: () => this.booksService.getGenres(),
  });

  // Resource para cargar la submission
  submissionId = this.route.snapshot.params['id'];
  submissionResource = rxResource({
    request: () => ({ id: this.submissionId }),
    loader: ({ request }) => this.booksService.getSubmission(request.id)
  });

  // Señales computadas
  genres = computed(() => this.genresResource.value() ?? []);
  submission = computed(() => this.submissionResource.value());
  isLoading = computed(() => this.submissionResource.isLoading());

  // Variables de estado
  isSubmitting = signal(false);

  bookForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    synopsis: ['', [Validators.required, Validators.maxLength(4000)]],
    genres: this.fb.array<number>([], [Validators.required]),
    volumeTitle: ['', [Validators.required]],
    volumeOrder: [1, [Validators.required, Validators.min(1)]],
    chapterTitle: ['', [Validators.required]],
    chapterOrder: [1, [Validators.required, Validators.min(1)]],
    chapterContent: ['', [Validators.required, Validators.minLength(10)]]
  });

  constructor() {
    // Usar effect para reaccionar a cambios en la submission
    effect(() => {
      const submission = this.submissionResource.value();
      if (submission) {
        this.populateForm(submission);
      }
    });
  }

  private populateForm(submission: any) {
    // Verificar que la submission puede ser editada
    if (submission.status !== 4) { // NeedsChanges = 4
      alert('Esta solicitud no puede ser editada. Solo las solicitudes que requieren cambios pueden ser editadas.');
      this.router.navigate(['/write/submissions', submission.id]);
      return;
    }

    this.bookForm.patchValue({
      title: submission.title,
      synopsis: submission.synopsis,
      volumeTitle: submission.volumeTitle,
      volumeOrder: submission.volumeOrder,
      chapterTitle: submission.chapterTitle,
      chapterOrder: submission.chapterOrder,
      chapterContent: submission.chapterContent
    });

    // Llenar géneros
    const genresArray = this.bookForm.get('genres') as FormArray;
    genresArray.clear();
    submission.genreIds.forEach((genreId: number) => {
      genresArray.push(new FormControl(genreId));
    });
  }

  isGenreSelected(genreId: number) {
    const selected = this.bookForm.get('genres') as FormArray;
    return selected.value.includes(genreId);
  }

  onGenreChange(event: Event) {
    const checkbox = event.target as HTMLInputElement;
    const genreId = Number(checkbox.value);
    const selected = this.bookForm.get('genres') as FormArray;

    if (checkbox.checked) {
      selected.push(new FormControl(genreId));
    } else {
      const index = selected.value.indexOf(genreId);
      if (index >= 0) {
        selected.removeAt(index);
      }
    }
  }

  onChapterContentChange(content: string) {
    this.bookForm.get('chapterContent')?.setValue(content);
    this.bookForm.get('chapterContent')?.markAsTouched();
  }

  async onSubmit() {
    if (this.bookForm.invalid) {
      this.markAllFieldsAsTouched();
      return;
    }

    this.isSubmitting.set(true);

    try {
      const submissionData: UpdateBookSubmissionDto = {
        title: this.bookForm.get('title')?.value,
        synopsis: this.bookForm.get('synopsis')?.value,
        genreIds: this.bookForm.get('genres')?.value,
        volumeTitle: this.bookForm.get('volumeTitle')?.value,
        volumeOrder: this.bookForm.get('volumeOrder')?.value,
        chapterTitle: this.bookForm.get('chapterTitle')?.value,
        chapterOrder: this.bookForm.get('chapterOrder')?.value,
        chapterContent: this.bookForm.get('chapterContent')?.value
      };

      const response = await this.booksService.updateSubmission(this.submissionId, submissionData).toPromise();

      alert('¡Solicitud actualizada correctamente! Los administradores la revisarán nuevamente.');
      this.router.navigate(['/write/submissions', this.submissionId]);

    } catch (error: any) {
      console.error('Error actualizando submission:', error);

      let errorMessage = 'Error al actualizar la solicitud';
      if (error.status === 401) {
        errorMessage = 'Debes iniciar sesión para editar una solicitud';
      } else if (error.status === 400) {
        errorMessage = error.error?.message || 'Datos inválidos. Verifica la información.';
      } else if (error.status === 403) {
        errorMessage = 'No tienes permisos para editar esta solicitud';
      } else if (error.status === 404) {
        errorMessage = 'Solicitud no encontrada';
      }

      alert(errorMessage);
    } finally {
      this.isSubmitting.set(false);
    }
  }

  private markAllFieldsAsTouched() {
    Object.keys(this.bookForm.controls).forEach(key => {
      const control = this.bookForm.get(key);
      control?.markAsTouched();
    });
  }

  isChapterContentInvalid(): boolean {
    const control = this.bookForm.get('chapterContent');
    return control ? (control.invalid && control.touched) : false;
  }

  getChapterContentError(): string {
    const control = this.bookForm.get('chapterContent');
    if (!control || !control.errors) return '';

    if (control.errors['required']) return 'El contenido del capítulo es requerido';
    if (control.errors['minlength']) return 'El contenido debe tener al menos 10 caracteres';

    return 'Contenido inválido';
  }

  cancelEdit() {
    this.router.navigate(['/write/submissions', this.submissionId]);
  }
}
