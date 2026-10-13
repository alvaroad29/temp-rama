import { CommonModule, DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterModule } from '@angular/router';
import { StatisticsService, AuthorStatisticsDto } from '../../../shared/services/statistics.service';

@Component({
  selector: 'app-author-dashboard-home',
  imports: [CommonModule, RouterModule, DatePipe],
  templateUrl: './authorDashboardHome.component.html',
})
export class AuthorDashboardHomeComponent {
  private statisticsService = inject(StatisticsService);

  // Señales para el estado
  isLoading = signal(true);
  error = signal<string | null>(null);
  statistics = signal<AuthorStatisticsDto | null>(null);

  constructor() {
    this.loadStatistics();
  }

  private loadStatistics(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.statisticsService.getAuthorStatistics().subscribe({
      next: (stats) => {
        this.statistics.set(stats);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Error al cargar las estadísticas');
        this.isLoading.set(false);
        console.error('Error loading author statistics:', err);
      }
    });
  }

  // Métodos de utilidad
  formatNumber(num: number): string {
    return num.toLocaleString('es-ES');
  }

  formatRating(rating: number): string {
    return rating.toFixed(1);
  }

  getDaysAgo(date: string | null): string {
    if (!date) return 'Nunca';

    const now = new Date();
    const past = new Date(date);
    const diffTime = Math.abs(now.getTime() - past.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays === 0) return 'Hoy';
    if (diffDays === 1) return 'Ayer';
    if (diffDays < 7) return `Hace ${diffDays} días`;
    if (diffDays < 30) return `Hace ${Math.floor(diffDays / 7)} semanas`;
    return `Hace ${Math.floor(diffDays / 30)} meses`;
  }

  getPerformanceClass(viewCount: number): string {
    if (viewCount >= 10000) return 'text-success';
    if (viewCount >= 1000) return 'text-warning';
    return 'text-info';
  }

  // getPerformanceBadge(viewCount: number): string {
  //   if (viewCount >= 10000) return 'badge badge-success';
  //   if (viewCount >= 1000) return 'badge badge-warning';
  //   return 'badge badge-info';
  // }

  reload(): void {
    this.loadStatistics();
  }
}
