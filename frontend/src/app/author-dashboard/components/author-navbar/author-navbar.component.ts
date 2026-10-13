import { CommonModule } from '@angular/common';
import { Component, computed, HostListener, inject, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';
import { ToggleThemeComponent } from '../../../shared/components/toggle-theme/toggle-theme.component';
import { LucideAngularModule, User } from 'lucide-angular';

@Component({
  selector: 'app-author-navbar',
  imports: [CommonModule, RouterLink, ToggleThemeComponent, LucideAngularModule],
  templateUrl: './author-navbar.component.html',
})
export class AuthorNavbarComponent {
  isSidebarOpen = input.required<boolean>();
  toggleSidebar = output<void>();

  onToggleSidebar() {
    this.toggleSidebar.emit();
  }

  authService = inject(AuthService);

  user = computed(() => this.authService.user());
  authStatus = computed(() => this.authService.authStatus());


  isMenuOpen = false;
  isUserDropdownOpen = false;
  lastScrollPosition = 0;
  isNavbarHidden = false;

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  toggleUserDropdown() {
    this.isUserDropdownOpen = !this.isUserDropdownOpen;
  }

  @HostListener('window:scroll')
  onWindowScroll() {
    const currentScrollPosition = window.pageYOffset;

    if (currentScrollPosition > 100) {
      this.isNavbarHidden = currentScrollPosition > this.lastScrollPosition;
    } else {
      this.isNavbarHidden = false;
    }

    this.lastScrollPosition = currentScrollPosition;
  }

  readonly userIcon = User;

}
