import { Routes } from '@angular/router';
import { AdminDashboardLayoutComponent } from './layout/admin-dashboard-layout/admin-dashboard-layout.component';
import { isAdminGuard } from '../auth/guards/is-admin.guard';
import { AnnoucementListPageComponent } from './pages/announcement/annoucement-list-page/annoucement-list-page.component';
import { CreateAnnouncementPageComponent } from './pages/announcement/create-announcement-page/create-announcement-page.component';
import { EditAnnouncementPageComponent } from './pages/announcement/edit-announcement-page/edit-announcement-page.component';
import { UserListPageComponent } from './pages/user/user-list-page/user-list-page.component';
import { AdminDashboardPageComponent } from './pages/dashboard/admin-dashboard-page/admin-dashboard-page.component';
import { AdminHomeCarouselPageComponent } from './pages/dashboard/admin-home-carousel-page/admin-home-carousel-page.component';
import { AdminSubmissionsPageComponent } from './pages/submission/admin-submissions-page/admin-submissions-page.component';
import { SubmissionDetailPageComponent } from '../author-dashboard/pages/submission-detail-page/submission-detail-page.component';
import { AdminBooksPageComponent } from './pages/book/admin-books-page/admin-books-page.component';
import { AdminReportsPageComponent } from './pages/report/admin-reports-page/admin-reports-page.component';
import { AdminGenresPageComponent } from './pages/genre/admin-genres-page/admin-genres-page.component';
import { CreateGenrePageComponent } from './pages/genre/create-genre-page/create-genre-page.component';
import { EditGenrePageComponent } from './pages/genre/edit-genre-page/edit-genre-page.component';

export const adminDashboardRoutes: Routes = [
  {
    path: '',
    component: AdminDashboardLayoutComponent,
    canMatch: [isAdminGuard],
    children: [
      {
        path: 'home',
        component: AdminDashboardPageComponent,
        title: 'Dashboard - Administración'
      },
      {
        path: 'home-carousel',
        component: AdminHomeCarouselPageComponent,
        title: 'Carousel - Administración'
      },
      {
        path: 'books',
        component: AdminBooksPageComponent,
        title: 'Libros - Administración'
      },
      // Gestión de usuarios
      {
        path: 'users',
        children: [
          {
            path: '',
            component: UserListPageComponent,
            title: 'Usuarios - Administración'
          },
          // {
          //   path: ':id',
          //   component: UserDetailPageComponent,
          //   title: 'Detalle de Usuario - Administración'
          // }
        ]
      },
      {
        path: 'submissions',
        component: AdminSubmissionsPageComponent,
        title: 'Solicitudes - Administración'
      },
      {
        path: 'submissions/:id',
        component: SubmissionDetailPageComponent,
        data: { context: 'admin' },
        title: 'Detalles de Solicitud - Administración'
      },
      {
        path: 'reports',
        component: AdminReportsPageComponent,
        title: 'Reportes - Administración'
      },
      {
        path: 'announcements',
        children: [
          {
            path: '',
            component: AnnoucementListPageComponent, // Lista de anuncios
            title: 'Anuncios - Administración'
          },
          {
            path: 'create',
            component: CreateAnnouncementPageComponent, // Crear nuevo anuncio
            title: 'Crear Anuncio - Administración'
          },
          {
            path: 'edit/:id',
            component: EditAnnouncementPageComponent, // Editar anuncio existente
            title: 'Editar Anuncio - Administración'
          }
        ]
      },
      {
        path: 'genres',
        children: [
          {
            path: '',
            component: AdminGenresPageComponent,
            title: 'Géneros - Administración'
          },
          {
            path: 'create',
            component: CreateGenrePageComponent,
            title: 'Crear Género - Administración'
          },
          {
            path: 'edit/:id',
            component: EditGenrePageComponent,
            title: 'Editar Género - Administración'
          }
        ]
      },
        {
        path: '**',
        redirectTo: 'home'
      }
    ],
  },

];

export default adminDashboardRoutes;
