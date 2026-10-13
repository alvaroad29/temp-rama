import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { rxResource } from '@angular/core/rxjs-interop';
import { catchError, of } from 'rxjs';
import { NgClass } from '@angular/common';
import { ListsService } from '../../../features/lists/services/lists.service';
import { BooksService } from '../../../features/books/services/books.service';
import { FormUtils } from '../../../auth/utils/form-utils';

@Component({
  selector: 'app-create-list-page',
  imports: [ReactiveFormsModule, NgClass, RouterLink],
  templateUrl: './create-list-page.component.html',
})
export class CreateListPageComponent {
  private listsService = inject(ListsService);
  private booksService = inject(BooksService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  formUtils = FormUtils;

  // Formulario (sin imageUrl en el form group, lo manejamos separado)
  listForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
    isPublic: [true],
    genreIds: [[]]
  });

  // Estados para la imagen
  selectedImage = signal<File | null>(null);
  imagePreview = signal<string | null>(null);
  imageError = signal<string | null>(null);

  // Estados generales
  successMessage = signal<string | null>(null);
  hasError = signal(false);
  errorMessages = signal<string[]>([]);
  isLoading = signal(false);

  // Resource para obtener géneros
  genresResource = rxResource({
    loader: () => {
      return this.booksService.getGenres().pipe(
        catchError(error => {
          console.error('Error loading genres:', error);
          return of([]);
        })
      );
    }
  });

  // Computed signals
  genres = computed(() => this.genresResource.value() || []);
  isLoadingGenres = computed(() => this.genresResource.isLoading());

  // Manejar selección de imagen
  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      const file = input.files[0];

      // Validar tipo de archivo
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp'];
      if (!allowedTypes.includes(file.type)) {
        this.imageError.set('Solo se permiten imágenes JPG, JPEG, PNG o WebP');
        this.clearImage();
        return;
      }

      // Validar tamaño (2MB = 2 * 1024 * 1024 bytes)
      if (file.size > 2 * 1024 * 1024) {
        this.imageError.set('La imagen no puede pesar más de 2MB');
        this.clearImage();
        return;
      }

      // Limpiar errores
      this.imageError.set(null);
      this.selectedImage.set(file);

      // Crear preview
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview.set(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  // Eliminar imagen seleccionada
  removeImage(): void {
    this.clearImage();

    // Resetear el input file
    const fileInput = document.getElementById('imageUpload') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  // Limpiar imagen
  private clearImage(): void {
    this.selectedImage.set(null);
    this.imagePreview.set(null);
    this.imageError.set(null);
  }

  onSubmit() {
    if (this.listForm.invalid) {
      this.listForm.markAllAsTouched();
      return;
    }

    this.clearErrors();
    this.isLoading.set(true);

    const { title, description, isPublic, genreIds } = this.listForm.value;
    const imageFile = this.selectedImage();

    this.listsService.createList(title, description, isPublic, genreIds, imageFile)
      .subscribe({
        next: (response: any) => {
          this.isLoading.set(false);

          if (response.success) {
            this.successMessage.set('Lista creada correctamente');
            this.listForm.reset({ isPublic: true, genreIds: [] });
            this.clearImage();

            // Redirigir después de 2 segundos
            setTimeout(() => {
              const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/user/lists';
              this.router.navigateByUrl(returnUrl);

            }, 2000);
          } else {
            this.setErrors(response.errors || ['Error desconocido']);
          }
        },
        error: (error) => {
          this.isLoading.set(false);
          this.setErrors(error.errors[0] || ['Ha ocurrido un error inesperado al crear la lista']);
          console.error('Error no manejado:', error);
        }
      });
  }

  // Manejar selección/deselección de géneros
  toggleGenre(genreId: number) {
    const currentGenres = this.listForm.get('genreIds')?.value || [];
    const index = currentGenres.indexOf(genreId);

    if (index > -1) {
      currentGenres.splice(index, 1);
    } else {
      currentGenres.push(genreId);
    }

    this.listForm.get('genreIds')?.setValue(currentGenres);
  }

  // Verificar si un género está seleccionado
  isGenreSelected(genreId: number): boolean {
    const currentGenres = this.listForm.get('genreIds')?.value || [];
    return currentGenres.includes(genreId);
  }

  private setErrors(errors: string[]) {
    this.hasError.set(true);
    this.errorMessages.set(errors);
  }

  private clearErrors() {
    this.hasError.set(false);
    this.errorMessages.set([]);
  }
}
