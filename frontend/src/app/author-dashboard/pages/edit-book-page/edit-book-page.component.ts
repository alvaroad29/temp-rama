import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { BooksService, UpdateBookDto } from '../../../features/books/services/books.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormUtils } from '../../../auth/utils/form-utils';
import { State } from '../../../features/books/interfaces/book-response.interface';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-edit-book-page',
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './edit-book-page.component.html',
})
export class EditBookPageComponent {
  private fb = inject(FormBuilder);
  private booksService = inject(BooksService);
  private route = inject(ActivatedRoute);
  router = inject(Router);
  formUtils = FormUtils;

  // Resource para cargar géneros
  genresResource = rxResource({
    loader: () => this.booksService.getGenres(),
  });

  // Resource para cargar el libro
  bookId = this.route.snapshot.params['id'];
  bookResource = rxResource({
    request: () => ({ id: this.bookId }),
    loader: ({ request }) => this.booksService.getBookSimple(request.id)
  });

  // Señales computadas
  genres = computed(() => this.genresResource.value() ?? []);
  book = computed(() => this.bookResource.value());
  isLoading = computed(() => this.bookResource.isLoading());

  // Variables de estado para la portada
  cover: File | null = null;
  coverPreview = signal<string | null>(null);
  coverUrl = signal<string | null>(null);
  isSubmitting = signal(false);
  isUploadingCover = signal(false);

  bookForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(3)]],
    description: ['', [Validators.required, Validators.maxLength(4000)]],
    state: [State.Activo, [Validators.required]],
    coverUrl: [''], // URL de la imagen subida
    genres: this.fb.array<number>([], [Validators.required])
  });

  constructor() {
    // Usar effect para reaccionar a cambios en el libro
    effect(() => {
      const book = this.bookResource.value();
      if (book) {
        this.populateForm(book);
      }
    });
  }

  private populateForm(book: any) {
    this.bookForm.patchValue({
      title: book.title,
      description: book.description,
      state: book.state,
      coverUrl: book.imageUrl || ''
    });

    // Establecer la URL de la portada existente
    if (book.imageUrl) {
      this.coverUrl.set(book.imageUrl);
      this.coverPreview.set(book.imageUrl); // Mostrar la imagen existente
    }

    // Llenar géneros
    const genresArray = this.bookForm.get('genres') as FormArray;
    genresArray.clear();
    book.genres.forEach((genre: any) => {
      genresArray.push(new FormControl(genre.id));
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

  // Manejar selección de portada
  async onCoverSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.cover = file;

      try {
        this.isUploadingCover.set(true);

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
        this.isUploadingCover.set(false);
      }
    }
  }

  // Eliminar portada subida
  async deleteCover() {
    if (!this.coverUrl()) return;

    try {
      // Solo eliminar del servidor si es una imagen recién subida
      // (podemos verificar si es una URL de Cloudinary o similar)
      if (this.coverUrl()?.includes('res.cloudinary.com')) {
        await this.booksService.deleteSubmissionCover(this.coverUrl()!).toPromise();
      }

      this.coverUrl.set(null);
      this.coverPreview.set(null);
      this.cover = null;
      this.bookForm.patchValue({ coverUrl: '' });
    } catch (error) {
      console.error('Error eliminando portada:', error);
      // Aún así limpiar la vista aunque falle la eliminación en el servidor
      this.coverUrl.set(null);
      this.coverPreview.set(null);
      this.cover = null;
      this.bookForm.patchValue({ coverUrl: '' });
    }
  }

  async onSubmit() {
    if (this.bookForm.invalid) {
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
      const bookData: UpdateBookDto = {
        title: this.bookForm.get('title')?.value,
        description: this.bookForm.get('description')?.value,
        state: this.bookForm.get('state')?.value,
        imageUrl: this.coverUrl() || undefined, // Usar la URL de la imagen subida o undefined
        genreIds: this.bookForm.get('genres')?.value
      };

      const response = await this.booksService.updateBook(Number(this.bookId), bookData).toPromise();

      alert('¡Libro actualizado correctamente!');
      this.router.navigate(['/write/books']);

    } catch (error: any) {
      console.error('Error actualizando libro:', error);

      let errorMessage = 'Error al actualizar el libro';
      if (error.status === 401) {
        errorMessage = 'Debes iniciar sesión para editar un libro';
      } else if (error.status === 400) {
        errorMessage = error.error?.message || 'Datos inválidos. Verifica la información.';
      } else if (error.status === 403) {
        errorMessage = 'No tienes permisos para editar este libro';
      } else if (error.status === 404) {
        errorMessage = 'Libro no encontrado';
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

  cancelEdit() {
    this.router.navigate(['/write/books']);
  }

  getStateOptions() {
    return [
      { value: State.Activo, label: 'Activo' },
      { value: State.Completado, label: 'Completado' },
      { value: State.Pausado, label: 'Pausado' }
    ];
  }

  // Método para verificar si hay una portada cargada
  hasCover(): boolean {
    return !!this.coverUrl();
  }
}
