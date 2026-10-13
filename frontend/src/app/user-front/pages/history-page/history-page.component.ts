import { Component, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { PaginationService } from '../../../shared/components/pagination/pagination.service';
import { HistorySearchService } from '../../services/history-search.service';
import { HistoryService } from '../../services/history.service';
import { HistorySearchComponent } from '../../../features/user/components/history-search/history-search.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-history-page',
  imports: [PaginationComponent, HistorySearchComponent, DatePipe, RouterLink],
  templateUrl: './history-page.component.html',
})
export class HistoryPageComponent {
 private historyService = inject(HistoryService);
  paginationService = inject(PaginationService);
  historySearchService = inject(HistorySearchService);

  historyResource = rxResource({
    request: () => ({
      page: this.paginationService.currentPage(),
      ...this.historySearchService.searchParams()
    }),
    loader: ({request}) => {
      console.log({request});
      return this.historyService.getHistory({
        PageNumber: request.page,
        BookTitle: request.BookTitle,
        FromDate: request.FromDate,
        ToDate: request.ToDate
      });
    },
  });

  // Computed signals
  history = computed(() => {
    const result = this.historyResource.value();
    return result?.data || [];
  });

  totalPages = computed(() => {
    const result = this.historyResource.value();
    return result?.totalPages || 0;
  });

  isLoading = computed(() => this.historyResource.isLoading());
  error = computed(() => this.historyResource.error());
//   historyData: HistoryItem[] = [
//   {
//     idBook: 'novel-001',
//     idChapter: 'ch-10',
//     nameChapter: 'capítulo 10 - la batalla final',
//     title: 'El ascenso del héroe',
//     image: 'https://placehold.co/80x80?text=Hero',
//     readAt: new Date('2025-07-20T14:30:00')
//   },
//   {
//     idBook: 'novel-002',
//     idChapter: 'ch-45',
//     nameChapter: 'capítulo 45 - el despertar del dragón',
//     title: 'Mundo de Dragones',
//     image: 'https://placehold.co/80x80?text=Dragon',
//     readAt: new Date('2025-07-18T10:00:00')
//   },
//   {
//     idBook: 'novel-003',
//     idChapter: 'ch-5',
//     nameChapter: 'capítulo 5 - sombras en la noche',
//     title: 'Cazadores Nocturnos',
//     image: 'https://placehold.co/80x80?text=Night',
//     readAt: new Date('2025-07-17T22:45:00')
//   },
//   {
//     idBook: 'novel-004',
//     idChapter: 'ch-21',
//     nameChapter: 'capítulo 21 - las ruinas antiguas',
//     title: 'Reinos Olvidados',
//     image: 'https://placehold.co/80x80?text=Ruins',
//     readAt: new Date('2025-07-15T18:00:00')
//   },
//   {
//     idBook: 'novel-005',
//     idChapter: 'ch-30',
//     nameChapter: 'capítulo 30 - el pacto oscuro',
//     title: 'Crónicas del Mago Oscuro',
//     image: 'https://placehold.co/80x80?text=Dark+Mage',
//     readAt: new Date('2025-07-14T09:15:00')
//   }
// ];
}
