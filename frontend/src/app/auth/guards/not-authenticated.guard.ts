import { inject } from '@angular/core';
import { CanMatchFn, Route, Router, UrlSegment } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { firstValueFrom } from 'rxjs';

export const NotAuthenticatedGuard: CanMatchFn = async (
  route: Route,
  segments: UrlSegment[]
) => {

  const authService = inject(AuthService);

  const router = inject(Router); // para redireccionar

  const isAuthenticated = await firstValueFrom( authService.checkStatus() ); // true o false

  if ( isAuthenticated ) {
    router.navigateByUrl('/');
    return false; // para que no vea la ruta
  }

  return true;
}
