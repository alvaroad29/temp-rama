import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { StatisticsService, AdminStatisticsDto } from '../../../../shared/services/statistics.service';

@Component({
  selector: 'app-admin-dashboard-page',
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-dashboard-page.component.html',
})
export class AdminDashboardPageComponent {
  private statisticsService = inject(StatisticsService);

  // Señales para el estado
  isLoading = signal(true);
  error = signal<string | null>(null);
  statistics = signal<AdminStatisticsDto | null>(null);

  constructor() {
    this.loadStatistics();
  }

  private loadStatistics(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.statisticsService.getAdminStatistics().subscribe({
      next: (stats) => {
        this.statistics.set(stats);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Error al cargar las estadísticas');
        this.isLoading.set(false);
        console.error('Error loading statistics:', err);
      }
    });
  }

  // Métodos de utilidad para formatear números
  formatNumber(num: number): string {
    return num.toLocaleString('es-ES');
  }

  // Método para recargar
  reload(): void {
    this.loadStatistics();
  }

  // Método para obtener el porcentaje de completitud
  getCompletionPercentage(current: number, total: number): number {
    return total > 0 ? Math.round((current / total) * 100) : 0;
  }

  // Método para obtener la clase de color según el estado
  getStatusBadgeClass(count: number, isAlert = false): string {
    if (isAlert && count > 0) return 'badge badge-error badge-sm whitespace-nowrap';
    if (count === 0) return 'badge badge-ghost badge-sm whitespace-nowrap';
    return 'badge badge-info badge-sm whitespace-nowrap';
  }
}
