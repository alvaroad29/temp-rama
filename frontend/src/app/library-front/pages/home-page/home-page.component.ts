import { Component, computed, inject, signal } from '@angular/core';
import { CarouselComponent } from "../../components/carousel/carousel.component";
import { BookCarouselComponent } from "../../../features/books/components/book-carousel/book-carousel.component";
import { CardData } from '../../../features/books/interfaces/card-data.interface';
import { RecentUpdatesComponent } from '../../../features/books/components/recent-updates/ recent-updates.component';
import { BooksService, SortBy } from '../../../features/books/services/books.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { Book, BooksResponse, State } from '../../../features/books/interfaces/book-response.interface';
import { BookPopularResponse } from '../../../features/books/interfaces/book-popular-response.interface';
import { AnnouncementComponent } from '../../components/announcement/announcement.component';
import { ListsService } from '../../../features/lists/services/lists.service';
import { catchError, forkJoin, map, of, tap } from 'rxjs';
import { BookList } from '../../../features/lists/interfaces/list-detailed.interface';

type Period = 'daily' | 'weekly' | 'monthly';

@Component({
  selector: 'app-home-page',
  imports: [CarouselComponent, BookCarouselComponent, RecentUpdatesComponent, AnnouncementComponent],
  templateUrl: './home-page.component.html',
})
export class HomePageComponent {

  private booksService = inject(BooksService);
  private listsService = inject(ListsService);

  booksResource = rxResource({
    request: () => ({}),
    loader: ({request}) => {
      return this.booksService.getPopularBooks();
    }
  });

  selectedPeriod = signal<Period>('daily');

  periods = [
    { label: 'Diario', value: 'daily' },
    { label: 'Semanal', value: 'weekly' },
    { label: 'Mensual', value: 'monthly' }
  ];

  cardsByPeriod = computed<Record<Period, CardData[]>>(() => {
    const data = this.booksResource.value();

    if (!data) return { daily: [], weekly: [], monthly: [] };

    const mapBooks = (books: Book[]): CardData[] =>
      books.map(book => ({
        title: book.title,
        image: book.imageUrl,
        status: State[book.state],
        score: book.score,
        id: book.id.toString(),
      }));

    return {
      daily: mapBooks(data.daily),
      weekly: mapBooks(data.weekly),
      monthly: mapBooks(data.monthly)
    };
  });

  changePeriod(period: Period) {
    this.selectedPeriod.set(period);
  }

  getPopularBooks = computed(() => this.cardsByPeriod()[this.selectedPeriod()]);

  // Resource para cargar los libros del carousel
//   carouselBooksResource = rxResource({
//     loader: () => {
//       console.log('🔄 Cargando libros del carousel...');
//       return this.listsService.getCarouselBooks().pipe(
//         tap(books => {
//           console.log('📚 Libros del carousel recibidos:', books);
//           console.log('📊 Cantidad de libros:', books?.length);
//         }),
//         catchError(error => {
//           console.error('❌ Error cargando carousel:', error);
//           return of([]);
//         })
//       );
//     }
//   });

//   fallbackBooksResource = rxResource({
//   loader: () => {
//     console.log('🔄 Cargando libros generales (fallback) con getBooks...');
//     return this.booksService.getBooks({
//       PageSize: 8,
//       SortBy: SortBy.Views,
//       SortDescending: true,
//       PageNumber: 1
//     }).pipe(
//       map(response => response.data),
//       tap(books => console.log('📚 Libros generales recibidos:', books)),
//       catchError(error => {
//         console.error('❌ Error cargando libros generales:', error);
//         return of([]);
//       })
//     );
//   }
// });

homeCarouselResource = rxResource({
  loader: () => {
    console.log('🔄 Cargando carousel y fallback en paralelo...');

    const carousel$ = this.listsService.getCarouselBooks().pipe(
      catchError(() => of([]))
    );

    const fallback$ = this.booksService.getBooks({
      PageSize: 8,
      SortBy: SortBy.Views,
      SortDescending: true,
      PageNumber: 1
    }).pipe(
      map(response => response.data),
      catchError(() => of([]))
    );

    return forkJoin({ carousel: carousel$, fallback: fallback$ }).pipe(
      tap(({ carousel, fallback }) => {
        console.log('📚 Carousel:', carousel?.length);
        console.log('📊 Fallback:', fallback?.length);
      })
    );
  }
});

  // Computed que usa los libros del carousel o los libros populares mensuales como fallback
  // getBooksHomeCarousel = computed(() => {
  //   const carouselBooks = this.carouselBooksResource.value();
  //   const fallbackBooks = this.fallbackBooksResource.value(); // 👈 USAR ESTE

  //   console.log('📖 getBooksHomeCarousel llamado');
  //   console.log('📚 Libros del carousel:', carouselBooks);
  //   console.log('📊 Libros generales (fallback):', fallbackBooks?.length);

  //   // Si hay libros en el carousel, usarlos
  //   if (carouselBooks && carouselBooks.length > 0) {
  //     console.log('✅ Usando libros del carousel');
  //     return carouselBooks;
  //   }

  //   // Si no, usar los libros generales ordenados por vistas
  //   if (fallbackBooks && fallbackBooks.length > 0) {
  //     console.log('🔄 Usando libros generales como fallback');

  //     // Convertir Book[] a BookList[]
  //     const convertedBooks: BookList[] = fallbackBooks.map((book, index) => ({
  //       id: book.id,
  //       title: book.title,
  //       imageUrl: book.imageUrl || '',
  //       description: book.description || '',
  //       score: book.score || 0,
  //       addedDate: new Date(),
  //       order: index,
  //       state: book.state
  //     }));

  //     return convertedBooks;
  //   }

  //   console.log('❌ No hay libros disponibles para el carousel');
  //   return [];
  // });

  getBooksHomeCarousel = computed(() => {
  const data = this.homeCarouselResource.value();

  if (!data) return [];

  // Si el carousel tiene libros, usarlos
  if (data.carousel && data.carousel.length > 0) {
    console.log('✅ Usando libros del carousel');
    return data.carousel;
  }

  // Si no, usar el fallback
  if (data.fallback && data.fallback.length > 0) {
    console.log('🔄 Usando libros generales como fallback');
    return data.fallback.map((book, index) => ({
      id: book.id,
      title: book.title,
      imageUrl: book.imageUrl || '',
      description: book.description || '',
      score: book.score || 0,
      addedDate: new Date(),
      order: index,
      state: book.state
    }));
  }

  return [];
});

// Método auxiliar para convertir el status string a State enum
private getStateFromStatus(status: string): State {
  const stateMap: { [key: string]: State } = {
    'Activo': State.Activo,
    'Completado': State.Completado,
    'Pausado': State.Pausado,

  };

  return stateMap[status] || State.Activo; // Valor por defecto
}

}
