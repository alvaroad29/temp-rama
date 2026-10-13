// front-navbar.component.ts
import { Component, computed, HostListener, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterModule } from '@angular/router';
import { LucideAngularModule, User } from 'lucide-angular';
import { ToggleThemeComponent } from "../../../shared/components/toggle-theme/toggle-theme.component";
import { AuthService } from '../../../auth/services/auth.service';


@Component({
  selector: 'front-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule, ToggleThemeComponent, RouterLink, RouterLinkActive],
  templateUrl: './front-navbar.component.html',
  // styleUrls: ['./front-navbar.component.css']
})
export class FrontNavbarComponent {
  authService = inject(AuthService);
  authStatus = this.authService.authStatus;
  currentUser = computed(() => this.authService.user());
  private router = inject(Router)

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

  onSearch(searchText: string) {
    if (searchText.trim()) {
      // Navegar a /library con el parámetro searchText
      this.router.navigate(['/library'], {
        queryParams: { searchText: searchText.trim() }
      });
    }
  }

  @ViewChild('userButton') userButton!: ElementRef;
  @ViewChild('userDropdown') userDropdown!: ElementRef;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    if (!this.isUserDropdownOpen) return;

    const clickedInsideButton = this.userButton?.nativeElement.contains(event.target);
    const clickedInsideDropdown = this.userDropdown?.nativeElement.contains(event.target);

    if (!clickedInsideButton && !clickedInsideDropdown) {
      this.isUserDropdownOpen = false;
    }
  }

  // Iconos
  readonly user = User;


}
