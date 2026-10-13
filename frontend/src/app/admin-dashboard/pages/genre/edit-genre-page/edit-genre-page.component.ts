import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormUtils } from '../../../../auth/utils/form-utils';
import { Genre } from '../../../../features/books/interfaces/book-response.interface';
import { ApiResponse } from '../../../../shared/services/annoucements.service';
import { BooksService } from '../../../../features/books/services/books.service';

@Component({
  selector: 'app-edit-genre-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './edit-genre-page.component.html',
})
export class EditGenrePageComponent {
  private genresService = inject(BooksService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  formUtils = FormUtils;

  genreForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
  });

  genreId = signal<number | null>(null);
  genre = signal<Genre | null>(null);
  successMessage = signal<string | null>(null);
  hasError = signal(false);
  errorMessages = signal<string[]>([]);
  isLoading = signal(false);
  isInitialLoading = signal(true);

  ngOnInit() {
    this.route.params.subscribe(params => {
      const id = +params['id'];
      if (id) {
        this.genreId.set(id);
        this.loadGenre(id);
      }
    });
  }

  loadGenre(id: number) {
    this.isInitialLoading.set(true);
    this.genresService.getGenre(id).subscribe({
      next: (genre) => {
        this.genre.set(genre);
        this.genreForm.patchValue({
          name: genre.name
        });
        this.isInitialLoading.set(false);
      },
      error: (error) => {
        this.setErrors(['Error al cargar el género']);
        this.isInitialLoading.set(false);
        console.error('Error loading genre:', error);
      }
    });
  }

  onSubmit() {
    if (this.genreForm.invalid || !this.genreId()) {
      this.genreForm.markAllAsTouched();
      return;
    }

    this.clearErrors();
    this.isLoading.set(true);

    const { name } = this.genreForm.value;

    this.genresService.updateGenre(this.genreId()!, name).subscribe({
      next: (response: ApiResponse) => {
        this.isLoading.set(false);

        if (response.success) {
          this.successMessage.set('Género actualizado correctamente');

          setTimeout(() => {
            this.router.navigate(['/admin/genres']);
          }, 2000);
        } else {
          this.setErrors(response.errors || ['Error desconocido']);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        this.setErrors(['Ha ocurrido un error inesperado']);
        console.error('Error no manejado:', error);
      }
    });
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
