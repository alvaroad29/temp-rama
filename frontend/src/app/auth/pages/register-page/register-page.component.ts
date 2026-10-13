import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormUtils } from '../../utils/form-utils';
import { Router, RouterLink } from '@angular/router';
import { AuthService, RegisterResponse } from '../../services/auth.service';

@Component({
  selector: 'register-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register-page.component.html',
})
export class RegisterPageComponent {
  authService = inject(AuthService);
  router = inject(Router);

  fb = inject(FormBuilder);

  formUtils = FormUtils;

  registerForm: FormGroup = this.fb.group({ // validaciones los mas parecidas a las del backend
    username: [
      '',
      [
        Validators.required,
        Validators.minLength(5),
        Validators.maxLength(20),
        Validators.pattern( FormUtils.usernamePattern )
      ]
    ],
    email: [
      '',
      [
        Validators.required,
        Validators.maxLength(254),
        Validators.pattern( FormUtils.emailPattern )
      ]
    ],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(100),
        FormUtils.passwordComplexityValidator
      ]
    ],
    confirmPassword: [
      '',
      [
        Validators.required
      ]
    ]
  },{
    validators: [
      FormUtils.isFieldOneEqualFieldTwo('password', 'confirmPassword')
    ]
  });


  hasError = signal(false);
  errorMessages = signal<string[]>([]);
  isLoading = signal(false);
  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return; // No seguimos si es inválido
    }
    // console.log(this.registerForm.value);

    this.clearErrors();
    this.isLoading.set(true);

    const { email, password, username } = this.registerForm.value;

    this.authService.register(email, password, username)
      .subscribe({
        next: (response: RegisterResponse) => {
          this.isLoading.set(false);

          if (response.success) {
            // Registro exitoso, redirigir al login
            this.router.navigateByUrl('/auth/login');
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
    // this.registerForm.reset();
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
