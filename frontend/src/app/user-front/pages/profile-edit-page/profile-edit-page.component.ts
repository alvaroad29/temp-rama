import { Component, inject, signal } from '@angular/core';
import { AuthService, RegisterResponse } from '../../../auth/services/auth.service';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormUtils } from '../../../auth/utils/form-utils';

@Component({
  selector: 'app-profile-edit-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './profile-edit-page.component.html',
})
export class ProfileEditPageComponent {
  authService = inject(AuthService);
  router = inject(Router);


  fb = inject(FormBuilder);
  formUtils = FormUtils;

  editUserForm: FormGroup = this.fb.group({
    username: [
      this.authService.user()?.userName || '',
      [
        Validators.minLength(5),
        Validators.maxLength(20),
        Validators.pattern( FormUtils.usernamePattern )
      ]
    ],
    newPassword: [
      '',
      [
        Validators.minLength(6),
        Validators.maxLength(100),
        FormUtils.passwordComplexityValidator
      ]
    ],
    confirmPassword: [
      '',
      [

      ]
    ],
    currentPassword: [
      '',
    ],
  },{
    validators: [
      FormUtils.isFieldOneEqualFieldTwo('newPassword', 'confirmPassword'),
      FormUtils.conditionalCurrentPasswordValidator('newPassword', 'currentPassword'),
      this.formUtils.isFieldOneNotEqualFieldTwo('newPassword', 'currentPassword')
    ]
  });


  user = this.authService.user();

  wasSaved = signal(false);
  hasError = signal(false);
  errorMessages = signal<string[]>([]);
  onSubmit() {
    if (this.editUserForm.invalid) {
      this.editUserForm.markAllAsTouched();
      return; // No seguimos si es inválido
    }

    this.clearErrors();

    const { username, newPassword, confirmPassword, currentPassword } = this.editUserForm.value;

    this.authService.editUser(this.user!.id, username, newPassword, confirmPassword, currentPassword )
      .subscribe(
        {
          next: (response: RegisterResponse) => {
            // this.isLoading.set(false);

            if (response.success) {
              // Registro exitoso,
              this.wasSaved.set(true);

              setTimeout(() => {
                this.wasSaved.set(false);
              }, 2000);
            } else {
              // Mostrar errores
              this.setErrors(response.errors || ['Error desconocido']);
            }
          },
          error: (error) => {
            // this.isLoading.set(false);
            this.setErrors(['Ha ocurrido un error inesperado']);
            console.error('Error no manejado:', error);
          }
        }
      )

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
