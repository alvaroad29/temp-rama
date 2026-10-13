import { Routes } from '@angular/router';
import { UserFrontLayoutComponent } from './layouts/user-front-layout/user-front-layout.component';
import { ProfilePageComponent } from './pages/profile-page/profile-page.component';
import { ListsPageComponent } from './pages/lists-page/lists-page.component';
import { HistoryPageComponent } from './pages/history-page/history-page.component';
import { ProfileEditPageComponent } from './pages/profile-edit-page/profile-edit-page.component';
import { CreateListPageComponent } from './pages/create-list-page/create-list-page.component';
import { EditListPageComponent } from './pages/edit-list-page/edit-list-page.component';
import { MyReportsPageComponent } from './pages/my-reports-page/my-reports-page.component';
import { FollowedListsPageComponent } from './pages/followed-lists-page/followed-lists-page.component';


export const userFrontRoutes: Routes = [
  {
    path: '',
    component: UserFrontLayoutComponent, // tiene el user-top-menu
    children: [
      {
        path: 'profile',
        component: ProfilePageComponent
      },
      {
        path: 'profile/edit',
        component: ProfileEditPageComponent
      },
      {
        path: 'history',
        component: HistoryPageComponent
      },
      {
        path: 'lists',
        component: ListsPageComponent
      },
      {
        path: 'followed-lists',
        component: FollowedListsPageComponent
      },
      {
        path: 'lists/new',
        component: CreateListPageComponent
      },
      {
        path: 'lists/edit/:id',
        component: EditListPageComponent
      },
       {
        path: 'reports',
        component: MyReportsPageComponent
      },
      {
        path: '**',
        redirectTo: 'profile'
      }
    ],
  },

];

export default userFrontRoutes; // para dsp exportarlo en app.routes.ts
