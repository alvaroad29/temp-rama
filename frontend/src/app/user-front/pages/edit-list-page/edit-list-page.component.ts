import { Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, of } from 'rxjs';
import { FormUtils } from '../../../auth/utils/form-utils';
import { Genre } from '../../../features/books/interfaces/book-response.interface';
import { BooksService } from '../../../features/books/services/books.service';
import { ListsService } from '../../../features/lists/services/lists.service';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-edit-list-page',
  imports: [ReactiveFormsModule, NgClass],
  templateUrl: './edit-list-page.component.html',
})
export class EditListPageComponent {
  private listsService = inject(ListsService);
  private booksService = inject(BooksService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);

  formUtils = FormUtils;

  // Obtener ID de la lista de la ruta
  listId = Number(this.activatedRoute.snapshot.params['id']);

  // Formulario
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
  currentImageUrl = signal<string | null>(null);
  removeImageFlag = signal(false);
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

  // Resource para obtener la lista actual
  listResource = rxResource({
    request: () => ({ id: this.listId }),
    loader: ({ request }) => {
      return this.listsService.getListById(request.id).pipe(
        catchError(error => {
          console.error('Error loading list:', error);
          this.errorMessages.set(['Error al cargar la lista']);
          return of(null);
        })
      );
    }
  });

  // Computed signals
  genres = computed(() => this.genresResource.value() || []);
  isLoadingGenres = computed(() => this.genresResource.isLoading());

  // Señal computada para la lista cargada (solo lectura)
  currentList = computed(() => this.listResource.value());

  // Señal computada para el estado de carga
  isLoadingList = computed(() => this.listResource.isLoading());

  // Effect para cargar datos cuando la lista esté disponible
  private loadListEffect = effect(() => {
    const list = this.currentList();
    if (list && !this.listForm.get('title')?.value) {
      this.loadListData(list);
    }
  });

  // Cargar datos de la lista en el formulario
  private loadListData(list: any): void {
    this.listForm.patchValue({
      title: list.title,
      description: list.description || '',
      isPublic: list.isPublic,
      genreIds: list.genres?.map((g: Genre) => g.id) || []
    });

    // Cargar imagen actual si existe
    if (list.imageUrl) {
      this.currentImageUrl.set(list.imageUrl);
      this.imagePreview.set(list.imageUrl);
    }
  }

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

 removeImage(): void {
  this.selectedImage.set(null);
  this.imagePreview.set(this.currentImageUrl()); // Volver a la imagen original
  this.imageError.set(null);

  const fileInput = document.getElementById('imageUpload') as HTMLInputElement;
  if (fileInput) {
    fileInput.value = '';
  }
}

 removeCurrentImage(): void {
  this.removeImageFlag.set(true);
  this.selectedImage.set(null);
  this.currentImageUrl.set(null);
  this.imagePreview.set(null);
  this.imageError.set(null);

  const fileInput = document.getElementById('imageUpload') as HTMLInputElement;
  if (fileInput) {
    fileInput.value = '';
  }
}

  private clearImage(): void {
    this.selectedImage.set(null);
    this.imagePreview.set(this.currentImageUrl());
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
  const removeImage = this.removeImageFlag();

  this.listsService.updateList(this.listId, title, description, isPublic, genreIds, imageFile, removeImage)
    .subscribe({
      next: (response: any) => {
        this.isLoading.set(false);
        this.removeImageFlag.set(false); // Resetear flag

        if (response.success) {
          this.successMessage.set('Lista actualizada correctamente');
          setTimeout(() => {
            this.router.navigate(['/user/lists']);
          }, 2000);
        } else {
          this.setErrors(response.errors || ['Error desconocido']);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        this.removeImageFlag.set(false); // Resetear flag
        this.setErrors(error.errors || ['Ha ocurrido un error inesperado al actualizar la lista']);
        console.error('Error no manejado:', error);
      }
    });
}
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
