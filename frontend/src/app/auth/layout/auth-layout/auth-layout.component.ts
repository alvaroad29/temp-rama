import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthNavbarComponent } from '../../components/auth-navbar/auth-navbar.component';

@Component({
  selector: 'auth-layout',
  imports: [RouterOutlet, AuthNavbarComponent],
  templateUrl: './auth-layout.component.html',
})
export class AuthLayoutComponent { }
