import { Component, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminNavbarComponent } from '../../components/admin-navbar/admin-navbar.component';
import { AdminSidebarComponent } from '../../components/admin-sidebar/admin-sidebar.component';
import { StatsCardsComponent } from '../../components/stats-cards/stats-cards.component';
import { MenuItem } from '../../interfaces/menu-item.interface';
import { StatsCard } from '../../interfaces/stats-card.interface';
import { RouterOutlet } from '@angular/router';


@Component({
  selector: 'app-admin-dashboard-layout',
  standalone: true,
  imports: [CommonModule, AdminNavbarComponent, AdminSidebarComponent, RouterOutlet],
  templateUrl: './admin-dashboard-layout.component.html',
})
export class AdminDashboardLayoutComponent {
  // Signals
  private _isSidebarOpen = signal(true);
  private _screenWidth = signal(window.innerWidth);

  // Computed signals
  isSidebarOpen = computed(() => this._isSidebarOpen());
  isMobile = computed(() => this._screenWidth() < 1024);
  sidebarWidth = computed(() => this.isMobile() ? '280px' : '240px');


  adminMenuItems: MenuItem[] = [
    {
      label: 'Dashboard',
      icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2H5a2 2 0 00-2-2z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 5a2 2 0 012-2h4a2 2 0 012 2v0H8v0z"></path></svg>`,
      route: '/admin/home'
    },
    {
      label: 'Carousel Home',
      icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h12a2 2 0 012 2v12a2 2 0 01-2 2H6a2 2 0 01-2-2V6zm2 0v12h12V6H6zm2 2h8v8H8V8z"></path></svg>`,
      route: '/admin/home-carousel'
    },
    {
      label: 'Usuarios',
      icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197m13.5-9a2.5 2.5 0 11-5 0 2.5 2.5 0 015 0z"></path></svg>`,
      route: '/admin/users'
    },
  ];

  booksMenuItems: MenuItem[] = [
 {
    label: 'Libros',
    icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"></path></svg>`,
    route: '/admin/books'
  },
    {
      label: 'Solicitudes',
      icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25zM6.75 12h.008v.008H6.75V12zm0 3h.008v.008H6.75V15zm0 3h.008v.008H6.75V18z"></path></svg>`,
      route: '/admin/submissions'
    },
    {
      label: 'Reportados',
      icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 8l6 6 6-6"></path></svg>`,
      route: '/admin/reports'
    },
    {
      label: 'Géneros',
      icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"></path></svg>`,
      route: '/admin/genres'
    },
  ];

  annoucementMenuItems: MenuItem[] = [
    {
      label: 'Anuncios',
      icon: `<svg fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-3.6-3.6A8.014 8.014 0 0017 10V9a8 8 0 10-16 0v1c0 1.566-.44 3.081-1.25 4.4L1 17h5m9 0v1a3 3 0 11-6 0v-1m6 0H9"></path></svg>`,
      route: '/admin/announcements'
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
