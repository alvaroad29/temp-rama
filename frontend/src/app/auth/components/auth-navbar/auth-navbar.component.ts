import { CommonModule } from '@angular/common';
import { Component, computed, HostListener, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterModule } from '@angular/router';
import { LucideAngularModule, User } from 'lucide-angular';
import { ToggleThemeComponent } from '../../../shared/components/toggle-theme/toggle-theme.component';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'auth-navbar',
  imports: [CommonModule, RouterModule, LucideAngularModule, ToggleThemeComponent, RouterLink, RouterLinkActive],
  templateUrl: './auth-navbar.component.html',
})
export class AuthNavbarComponent {
  // authService = inject(AuthService);
  // authStatus = this.authService.authStatus;
  // currentUser = computed(() => this.authService.user());
  // private router = inject(Router)

  // isMenuOpen = false;
  // isUserDropdownOpen = false;
  // lastScrollPosition = 0;
  isNavbarHidden = false;

  // toggleMenu() {
  //   this.isMenuOpen = !this.isMenuOpen;
  // }

  // toggleUserDropdown() {
  //   this.isUserDropdownOpen = !this.isUserDropdownOpen;
  // }

  // @HostListener('window:scroll')
  // onWindowScroll() {
  //   const currentScrollPosition = window.pageYOffset;

  //   if (currentScrollPosition > 100) {
  //     this.isNavbarHidden = currentScrollPosition > this.lastScrollPosition;
  //   } else {
  //     this.isNavbarHidden = false;
  //   }

  //   this.lastScrollPosition = currentScrollPosition;
  // }

  // onSearch(searchText: string) {
  //   if (searchText.trim()) {
  //     // Navegar a /library con el parámetro searchText
  //     this.router.navigate(['/library'], {
  //       queryParams: { searchText: searchText.trim() }
  //     });
  //   }
  // }


  // Iconos
  readonly user = User;
}
