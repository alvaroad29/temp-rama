import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UserTopMenuComponent } from '../../../features/user/components/user-top-menu/user-top-menu.component';

@Component({
  selector: 'user-front-layout',
  imports: [RouterOutlet, UserTopMenuComponent],
  templateUrl: './user-front-layout.component.html',
})
export class UserFrontLayoutComponent { }
