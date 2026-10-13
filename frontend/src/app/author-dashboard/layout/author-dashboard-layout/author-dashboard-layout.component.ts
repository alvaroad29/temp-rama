import { Component, computed, HostListener, signal } from '@angular/core';
import { MenuItem } from '../../../admin-dashboard/interfaces/menu-item.interface';
import { StatsCard } from '../../../admin-dashboard/interfaces/stats-card.interface';
import { AuthorNavbarComponent } from '../../components/author-navbar/author-navbar.component';
import { AuthorSidebarComponent } from '../../components/author-sidebar/author-sidebar.component';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-author-dashboard-layout',
  imports: [CommonModule, AuthorNavbarComponent, AuthorSidebarComponent, RouterOutlet],
  templateUrl: './author-dashboard-layout.component.html',
})
export class AuthorDashboardLayoutComponent {
  private _isSidebarOpen = signal(true);
  private _screenWidth = signal(window.innerWidth);

  // Computed signals
  isSidebarOpen = computed(() => this._isSidebarOpen());
  isMobile = computed(() => this._screenWidth() < 1024);
  sidebarWidth = computed(() => this.isMobile() ? '280px' : '240px');

  // Menu items (podrían ser inputs si se desea)
  authorMenuItems: MenuItem[] = [
    {
      label: 'Dashboard',
      icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2H5a2 2 0 00-2-2z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5a2 2 0 012-2h4a2 2 0 012 2v0H8v0z"></path></svg>`,
      route: '/write/home'
    },
    {
      label: 'Publicados',
      icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path></svg>`,
      route: '/write/books'
      // children: []
    },
    {
      label: 'Mis Solicitudes',
      icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25zM6.75 12h.008v.008H6.75V12zm0 3h.008v.008H6.75V15zm0 3h.008v.008H6.75V18z"></path></svg>`,
      route: '/write/submissions'
    }
  ];

  // Stats cards (podrían ser inputs si se desea)
  statsCards: StatsCard[] = [
    {
      title: 'Users',
      value: '2',
      link: 'View'
    },
    {
      title: 'Companies',
      value: '100',
      change: '+30%',
      changeType: 'positive',
      link: 'View'
    },
    {
      title: 'Blogs',
      value: '100',
      link: 'View'
    }
  ];

  // Activities (podrían ser inputs si se desea)
  activities = [
    { title: 'Lorem Ipsum', date: '02-02-2024', time: '17:45' },
    { title: 'Lorem Ipsum', date: '02-02-2024', time: '17:45' }
  ];

   constructor() {
    this._isSidebarOpen.set(!this.isMobile());
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    this._screenWidth.set(event.target.innerWidth);
    if (this.isMobile() && this.isSidebarOpen()) {
      this._isSidebarOpen.set(false);
    }
  }

  toggleSidebar() {
    this._isSidebarOpen.update(open => !open);
  }

  closeSidebar() {
    if (this.isMobile()) {
      this._isSidebarOpen.set(false);
    }
  }
}
