import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { FormUtils } from '../../../../auth/utils/form-utils';
import { AnnoucementsService, ApiResponse } from '../../../../shared/services/annoucements.service';

@Component({
  selector: 'app-create-announcement-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './create-announcement-page.component.html',
})
export class CreateAnnouncementPageComponent {

  private annoucementService = inject(AnnoucementsService);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  formUtils = FormUtils;

  announcementForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    content: ['', [Validators.required, Validators.maxLength(5000)]],
  });


  successMessage = signal<string | null>(null);

  hasError = signal(false);
  errorMessages = signal<string[]>([]);
  isLoading = signal(false);
  onSubmit() {
     if (this.announcementForm.invalid) {
      this.announcementForm.markAllAsTouched();
      return;
    }

    this.clearErrors();
    this.isLoading.set(true);

    const { title, content }  = this.announcementForm.value;
    console.log(title, content);

    this.annoucementService.createAnnoucement(title, content)
      .subscribe({
        next: (response: ApiResponse) => {
          this.isLoading.set(false);

          if (response.success) {
            this.successMessage.set('Anuncio creado correctamente');

            this.announcementForm.reset();
          // Redirigir después de 2 segundos
          setTimeout(() => {
            this.router.navigate(['/admin/announcements']);
          }, 2000);
        } else {
            // Mostrar errores
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
