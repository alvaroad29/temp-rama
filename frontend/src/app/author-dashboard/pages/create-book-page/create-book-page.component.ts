import { Component, computed, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { FormUtils } from '../../../auth/utils/form-utils';
import { BooksService, CreateBookSubmissionDto } from '../../../features/books/services/books.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { MarkdownEditorComponent } from '../../../features/books/components/markdown-editor/markdown-editor.component';

@Component({
  selector: 'app-create-book-page',
  imports: [ReactiveFormsModule, RouterLink, MarkdownEditorComponent],
  templateUrl: './create-book-page.component.html',
})
export class CreateBookPageComponent {

   private fb = inject(FormBuilder);
  private booksService = inject(BooksService);
  private router = inject(Router);
  formUtils = FormUtils;

  // Resource para cargar géneros
  genresResource = rxResource({
      loader: () => {
          return this.booksService.getGenres();
        },
      }
    );

  // Señal computada para los géneros
  genres = computed(() => this.genresResource.value() ?? []);

  // Variables de estado
  cover: File | null = null;
  coverPreview = signal<string | null>(null);
  coverUrl = signal<string | null>(null); // Nueva señal para la URL de la portada subida
  isSubmitting = signal(false); // Nueva señal para estado de envío

  bookForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    coverUrl: [''], // Ahora será la URL de la imagen subida
    synopsis: ['', [Validators.required, Validators.maxLength(4000)]],
    genres: this.fb.array<number>([], [Validators.required]),

    volumeTitle: ['', [Validators.required]],
    volumeOrder: [1, [Validators.required, Validators.min(1)]],

    chapterTitle: ['', [Validators.required]],
    chapterOrder: [1, [Validators.required, Validators.min(1)]],
    chapterContent: ['', [Validators.required, Validators.minLength(500), Validators.maxLength(20000)]]
  });

  hasError = () => false;
  errorMessages = () => [];

  isGenreSelected(genreId: number) {
    const selected = this.bookForm.get('genres') as FormArray;
    return selected.value.includes(genreId);
  }

  onGenreChange(event: Event) {
    const checkbox = event.target as HTMLInputElement;
    const genreId = Number(checkbox.value); // Convertir a número
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

  // Manejar selección de portada - AHORA SUBE LA IMAGEN
  async onCoverSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.cover = file;

      try {
        this.isSubmitting.set(true);

        // Subir la imagen y obtener la URL
        const response = await this.booksService.uploadSubmissionCover(file).toPromise();
        this.coverUrl.set(response!.url);

        // Actualizar el formulario con la URL
        this.bookForm.patchValue({ coverUrl: response!.url });

        // Vista previa local
        const reader = new FileReader();
        reader.onload = () => this.coverPreview.set(reader.result as string);
        reader.readAsDataURL(file);

      } catch (error) {
        console.error('Error subiendo portada:', error);
        alert('Error al subir la portada. Intenta nuevamente.');
        this.cover = null;
        input.value = ''; // Limpiar el input
      } finally {
        this.isSubmitting.set(false);
      }
    }
  }

  // Eliminar portada subida
  async deleteCover() {
    if (!this.coverUrl()) return;

    try {
      await this.booksService.deleteSubmissionCover(this.coverUrl()!).toPromise();
      this.coverUrl.set(null);
      this.coverPreview.set(null);
      this.cover = null;
      this.bookForm.patchValue({ coverUrl: '' });
    } catch (error) {
      console.error('Error eliminando portada:', error);
    }
  }

  // Manejar cambios en el contenido del markdown editor
  onChapterContentChange(content: string) {
    this.bookForm.get('chapterContent')?.setValue(content);
    this.bookForm.get('chapterContent')?.markAsTouched();
  }

  async onSubmit() {
    console.log('Formulario enviado:', this.bookForm.value);

    if (this.bookForm.invalid) {
      console.log('Formulario inválido');
      this.markAllFieldsAsTouched();
      return;
    }

    // Verificar que se haya subido una portada si se seleccionó
    if (this.cover && !this.coverUrl()) {
      alert('Espera a que se termine de subir la portada');
      return;
    }

    this.isSubmitting.set(true);

    try {
      const submissionData: CreateBookSubmissionDto = {
        title: this.bookForm.get('title')?.value,
        coverUrl: this.coverUrl(), // URL de la imagen subida o null
        synopsis: this.bookForm.get('synopsis')?.value,
        genreIds: this.bookForm.get('genres')?.value,
        volumeTitle: this.bookForm.get('volumeTitle')?.value,
        volumeOrder: this.bookForm.get('volumeOrder')?.value,
        chapterTitle: this.bookForm.get('chapterTitle')?.value,
        chapterOrder: this.bookForm.get('chapterOrder')?.value,
        chapterContent: this.bookForm.get('chapterContent')?.value
      };

      const response = await this.booksService.createBookSubmission(submissionData).toPromise();

      console.log('Submission creada:', response);
      alert('¡Solicitud enviada correctamente! Los administradores la revisarán pronto.');

      // Redirigir a la página de mis submissions o a dashboard
      this.router.navigate(['/write/submissions']);

    } catch (error: any) {
      console.error('Error creando submission:', error);

      let errorMessage = 'Error al enviar la solicitud';
      if (error.status === 401) {
        errorMessage = 'Debes iniciar sesión para enviar una solicitud';
      } else if (error.status === 400) {
        errorMessage = error.error?.message || 'Datos inválidos. Verifica la información.';
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

  // Método para verificar si el campo de contenido tiene error
  isChapterContentInvalid(): boolean {
    const control = this.bookForm.get('chapterContent');
    return control ? (control.invalid && control.touched) : false;
  }

  getChapterContentError(): string {
    const control = this.bookForm.get('chapterContent');
    if (!control || !control.errors) return '';

    if (control.errors['required']) return 'El contenido del capítulo es requerido';
    if (control.errors['minlength']) return 'El contenido debe tener al menos 500 caracteres';
    if (control.errors['maxlength']) return 'El contenido no puede exceder los 20.000 caracteres';

    return 'Contenido inválido';
  }

  resetForm() {
    this.bookForm.reset({
      volumeOrder: 1,
      chapterOrder: 1
    });
    this.cover = null;
    this.coverPreview.set(null);
    this.coverUrl.set(null);

    // Resetear el array de géneros
    const genresArray = this.bookForm.get('genres') as FormArray;
    while (genresArray.length !== 0) {
      genresArray.removeAt(0);
    }
  }
}
