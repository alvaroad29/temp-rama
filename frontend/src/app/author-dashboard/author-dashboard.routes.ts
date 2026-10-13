import { Routes } from '@angular/router';
import { AuthorDashboardLayoutComponent } from './layout/author-dashboard-layout/author-dashboard-layout.component';
import { AuthorDashboardHomeComponent } from './pages/authorDashboardHome/authorDashboardHome.component';
import { MyBooksPageComponent } from './pages/my-books-page/my-books-page.component';
import { CreateBookPageComponent } from './pages/create-book-page/create-book-page.component';
import { MySubmissionsPageComponent } from './pages/my-submissions-page/my-submissions-page.component';
import { SubmissionDetailPageComponent } from './pages/submission-detail-page/submission-detail-page.component';
import { EditSubmissionPageComponent } from './pages/edit-submission-page/edit-submission-page.component';
import { EditBookPageComponent } from './pages/edit-book-page/edit-book-page.component';
import { BookVolumesPageComponent } from './pages/book-volumes-page/book-volumes-page.component';
import { VolumeChaptersPageComponent } from './pages/volume-chapters-page/volume-chapters-page.component';


export const authorDashboardRoutes: Routes = [
  {
    path: '',
    component: AuthorDashboardLayoutComponent,
    children: [
      {
        path: 'home',
        component: AuthorDashboardHomeComponent
      },
      {
        path: 'books',
        component: MyBooksPageComponent,
      },
      {
        path: 'books/new',
        component: CreateBookPageComponent,

      },
      {
        path: 'books/:id/edit',
        component: EditBookPageComponent,
      },
      {
        path: 'books/:id/volumes',
        component: BookVolumesPageComponent,
      },
      {
        path: 'books/:bookId/volumes/:volumeId/chapters',
        component: VolumeChaptersPageComponent,
      },
      {
        path: 'submissions',
        component: MySubmissionsPageComponent,
         data: { context: 'author' },
      },
      {
        path: 'submissions/:id',
        component: SubmissionDetailPageComponent
      },
      {
        path: 'submissions/:id/edit',
        component: EditSubmissionPageComponent
      },

      {
        path: '**',
        redirectTo: 'home'
      }
    ],
  },

];

export default authorDashboardRoutes;
