import { Component, computed, inject, input, linkedSignal, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'shared-pagination',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule],
  templateUrl: './pagination.component.html',
})
export class PaginationComponent {
  private router = inject(Router);
  currentPage = input<number>(1);
  pages = input<number>(1);

  activePage = linkedSignal(this.currentPage);
  customPage = signal<number | null>(null);

  getPagesList = computed(() => {
    const totalPages = this.pages();
    const current = this.activePage();
    const pages: (number | string)[] = [];

    pages.push(1);

    const rangeStart = Math.max(2, current - 2);
    const rangeEnd = Math.min(totalPages - 1, current + 2);

    if (rangeStart > 2) {
      pages.push('...left');
    }

    for (let i = rangeStart; i <= rangeEnd; i++) {
      pages.push(i);
    }

    if (rangeEnd < totalPages - 1) {
      pages.push('...right');
    }

    if (totalPages > 1) {
      pages.push(totalPages);
    }

    return pages;
  });

  hasPrev = computed(() => this.activePage() > 1);
  hasNext = computed(() => this.activePage() < this.pages());

   // Método para navegar manteniendo todos los query params
  navigateToPage(page: number) {
  this.router.navigate([], {
    queryParams: { page },
    queryParamsHandling: 'merge'
  }).then(() => {
    // Opcional: cualquier lógica post-navegación
  });
}


  goToPage() {
    let page = this.customPage();

    // Si no es un número válido, usar página 1
    if (page === null || isNaN(page)) {
      page = 1;
    }

    // Truncar decimales (ej: 3.9 → 3)
    page = Math.trunc(page);

    // Forzar límites: mínimo 1, máximo pages()
    page = Math.max(1, Math.min(page, this.pages()));

    // Navegar programáticamente
    this.navigateToPage(page);

    // Resetear el input y cerrar dropdown
    this.customPage.set(null);
    (document.activeElement as HTMLElement)?.blur();
  }
}
