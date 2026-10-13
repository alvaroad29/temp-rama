import { inject } from '@angular/core';
import { CanMatchFn, Route, Router, UrlSegment } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { firstValueFrom } from 'rxjs';

export const isAdminGuard: CanMatchFn = async (
  route: Route,
  segments: UrlSegment[]
) => {

  const authService = inject(AuthService);
  const router = inject(Router); // para redireccionar

  await firstValueFrom( authService.checkStatus() ); // true o false

  const isAdmin = authService.isAdmin();

  if ( !isAdmin ) {
    router.navigateByUrl('/');
    return false; // para que no vea la ruta
  }

  return true;
}
