import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { FormUtils } from '../../../../auth/utils/form-utils';
import { ApiResponse } from '../../../../shared/services/annoucements.service';
import { BooksService } from '../../../../features/books/services/books.service';

@Component({
  selector: 'app-create-genre-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './create-genre-page.component.html',
})
export class CreateGenrePageComponent {
    private genresService = inject(BooksService);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  formUtils = FormUtils;

  genreForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50)]],
  });

  successMessage = signal<string | null>(null);
  hasError = signal(false);
  errorMessages = signal<string[]>([]);
  isLoading = signal(false);

  onSubmit() {
    if (this.genreForm.invalid) {
      this.genreForm.markAllAsTouched();
      return;
    }

    this.clearErrors();
    this.isLoading.set(true);

    const { name } = this.genreForm.value;

    this.genresService.createGenre(name).subscribe({
      next: (response: ApiResponse) => {
        this.isLoading.set(false);

        if (response.success) {
          this.successMessage.set('Género creado correctamente');
          this.genreForm.reset();

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
