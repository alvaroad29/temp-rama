import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormUtils } from '../../utils/form-utils';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'login-page',
  imports: [ RouterLink, ReactiveFormsModule],
  templateUrl: './login-page.component.html',
})
export class LoginPageComponent {

  authService = inject(AuthService);
  router = inject(Router);
  private route = inject(ActivatedRoute);



  fb = inject(FormBuilder);

  formUtils = FormUtils;

  loginForm: FormGroup = this.fb.group({ // validaciones los mas parecidas a las del backend
    email: ['', [Validators.required, Validators.pattern(FormUtils.emailPattern)]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  })

  hasError = signal(false);
  isLoading = signal(false);
  isBanned = signal(false);
  banMessage = signal('');

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.hasError.set(false);
    this.isBanned.set(false);
    this.banMessage.set('');
    this.isLoading.set(true);

    const { email, password } = this.loginForm.value;

    this.authService.login(email, password)
      .subscribe( result  => {
        this.isLoading.set(false);
        if (result === true) {
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
          this.router.navigateByUrl(returnUrl);
          return;
        }
         else if (typeof result === 'string') {
        // Usuario baneado
        this.isBanned.set(true);
        this.banMessage.set(result);
        this.hasError.set(true);
      }
      else {
        // Otro error (credenciales incorrectas)
        this.hasError.set(true);
        this.isBanned.set(false);
      }
      });

    // this.loginForm.reset();
  }



}
