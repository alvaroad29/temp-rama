import { Routes } from '@angular/router';
import { NotAuthenticatedGuard } from './auth/guards/not-authenticated.guard';
import { AuthenticatedGuard } from './library-front/guards/authenticated.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./auth/auth.routes'),
    canMatch: [
      NotAuthenticatedGuard
    ]
  },
  {
    path: 'admin',
    loadChildren: () => import('./admin-dashboard/admin-dashboard.routes'),
  },
  {
    path: 'write',
    loadChildren: () => import('./author-dashboard/author-dashboard.routes'),
    canMatch: [AuthenticatedGuard]
  },
  {
    path: '',
    loadChildren: () => import('./library-front//library-front.routes'),
  },
];
