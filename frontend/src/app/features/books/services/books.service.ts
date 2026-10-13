import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Book, BooksResponse, Genre, State } from '../interfaces/book-response.interface';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { BookPopularResponse } from '../interfaces/book-popular-response.interface';
import { environment } from '../../../../environments/environment';
import { BookDetails } from '../interfaces/book-details-interface';
import { ReadChapter } from '../interfaces/chapter-interface';
import { CardUpdate } from '../interfaces/card-update.interface';

const baseUrl = environment.baseUrl;

export interface BookRatingResponseDto {
  id: number;
  bookId: number;
  userId: string;
  ratingValue: number;
  ratingDate: string;
  username: string;
}

export interface UserRatingDto {
  rating: number | null;
}

export interface RatingRequestDto {
  rating: number;
}

export interface BookQueryParameters
{
    // Paginación
    PageNumber?: number;
    PageSize?: number;

    // Filtros
    SearchText?: string;
    SearchIn?: SearchInField; // Campo para buscar
    State?: State;
    GenreIds?: number[]; // IDs de géneros a filtrar

    // Ordenamiento
    SortBy?: SortBy; // "title", "score", "description"
    SortDescending?: boolean; // "asc", "desc"
}

export enum SortBy {
  Title = 'title',
  Score = 'score',
  Views = 'viewcount'
}

export type SortByOption = {
  value: SortBy;
  label: string;
};

export enum SearchInField {
  All = 'all',
  Title = 'title',
  Author = 'author',
  Description = 'description',
}

export type SearchFieldOption = {
  value: SearchInField;
  label: string;
};

export interface CreateBookSubmissionDto {
  title: string;
  coverUrl: string|null;
  synopsis: string;
  genreIds: number[];
  volumeTitle: string;
  volumeOrder: number;
  chapterTitle: string;
  chapterOrder: number;
  chapterContent: string;
}

export interface BookSubmissionResponseDto {
  id: number;
  title: string;
  coverUrl?: string;
  synopsis: string;
  genreIds: number[];
  volumeTitle: string;
  volumeOrder: number;
  chapterTitle: string;
  chapterOrder: number;
  chapterContent: string;
  status: BookSubmissionStatus;
  adminFeedback?: string;
  createdAt: string;
  updatedAt: string;
  userId?: string;
  userName?: string;
  reviewedByAdminName?: string;
  reviewedAt?: string;
}

export interface UpdateBookSubmissionStatusDto {
  status: BookSubmissionStatus;
  adminFeedback?: string;
}

export enum BookSubmissionStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  NeedsChanges = 4
}

export interface UpdateBookSubmissionDto {
  title: string;
  synopsis: string;
  genreIds: number[];
  volumeTitle: string;
  volumeOrder: number;
  chapterTitle: string;
  chapterOrder: number;
  chapterContent: string;
}

export interface DeactivateBookDto {
  isActive: boolean;
}

export interface UpdateBookDto {
  title: string;
  description: string;
  genreIds: number[];
  state: State;
  imageUrl?: string;
}

export interface VolumeSimpleDto {
  id: number;
  title: string;
  order: number;
  chapterCount: number;
  isActive: boolean;
}

export interface VolumeCreateDto {
  title: string;
  order: number;
  bookId: number;
}

export interface VolumeUpdateDto {
  title?: string;
  order?: number;
  isActive?: boolean;
}

export interface ChapterDto {
  id: number;
  title: string;
  viewCount: number;
  publishDate: string;
  order: number;
  isActive: boolean;
}

export interface CreateChapterDto {
  title: string;
  volumeId: number;
  order: number;
  content: string;
  isActive?: boolean;
}

export interface UpdateChapterDto {
  title?: string;
  order?: number;
  content?: string;
  isActive?: boolean;
}

export interface ChapterAdminDto {
  id: number;
  title: string;
  order: number;
  content: string;
  viewCount: number;
  publishDate: string;
  updateDate?: string;
  isActive: boolean;
  volumeId: number;
}

export interface CreateGenreDto {
  name: string;
}

export interface ApiResponse<T = any> {
  success: boolean;
  data?: T;
  errors?: string[];
  message?: string;
}

export interface SubmissionStatsDto {
  Pending: number;
  Approved: number;
  Rejected: number;
  NeedsChanges: number;
}

@Injectable({
  providedIn: 'root'
})
export class BooksService {

  private http = inject(HttpClient);


  getPopularBooks(): Observable<BookPopularResponse> {
    return this.http.get<BookPopularResponse>(`${baseUrl}/Book/popular/all`)
    // .pipe( tap (resp => console.log(resp)));
  }

  getBooks(options: BookQueryParameters): Observable<BooksResponse> {


    const defaultedOptions = {
      PageNumber: 1,
      PageSize: 8,
      SortDescending: false,
      SearchIn: SearchInField.Title,
      SortBy: SortBy.Title,
      ...options
    };

    // Si State es All, lo eliminamos (no queremos enviarlo)
    if (defaultedOptions.State === State.All) {
      delete defaultedOptions.State;
    }

    // Filtrar undefined antes de enviar
    const params = Object.fromEntries(
      Object.entries(defaultedOptions)
        .filter(([_, value]) => value !== undefined)
    );

    console.log({params});
    return this.http.get<BooksResponse>(`${baseUrl}/Book`, { params });
      // .pipe( tap (resp => console.log(resp)));
  }

  deactivateBook(id: number, isActive: boolean): Observable<{message: string, isActive: boolean}> {
    return this.http.patch<{message: string, isActive: boolean}>(
      `${baseUrl}/Book/${id}/deactivate`,
      { isActive }
    );
  }

// Método específico para admin que incluye libros inactivos
getBooksForAdmin(options: BookQueryParameters): Observable<BooksResponse> {
  const defaultedOptions = {
    PageNumber: 1,
    PageSize: 30, // Más libros para admin
    SortDescending: false,
    SearchIn: SearchInField.Title,
    SortBy: SortBy.Title,
    ...options
  };

  // Si State es All, lo eliminamos
  if (defaultedOptions.State === State.All) {
    delete defaultedOptions.State;
  }

  // Filtrar undefined antes de enviar
  const params = Object.fromEntries(
    Object.entries(defaultedOptions)
      .filter(([_, value]) => value !== undefined)
  );

  return this.http.get<BooksResponse>(`${baseUrl}/Book`, { params });
}

  getRecentBooksWithChapters(): Observable<CardUpdate[]> {
     return this.http.get<CardUpdate[]>(`${baseUrl}/Book/recent-updates`)
  }

  getBook(id: number): Observable<BookDetails> {
    return this.http.get<BookDetails>(`${baseUrl}/Book/${id}/detailed`)
    // .pipe(
    //   tap( resp=> console.log(resp)),
    // )

  }

  getChapter(id: number): Observable<ReadChapter> {
    return this.http.get<ReadChapter>(`${baseUrl}/chapter/${id}`)
    // .pipe(
    //   tap( resp=> console.log(resp)),
    // )

  }

  getGenres(): Observable<Genre[]> {
    return this.http.get<Genre[]>(`${baseUrl}/Genre`)
    // .pipe(
    //   tap(r => {
    //   console.log(r);
    //   })
    // );
  }

   getAllGenresForAdmin(): Observable<Genre[]> {
    return this.http.get<Genre[]>(`${baseUrl}/Genre/admin/all`);
  }

  getGenre(id: number): Observable<Genre> {
    return this.http.get<Genre>(`${baseUrl}/Genre/${id}`);
  }

  createGenre(name: string): Observable<ApiResponse<Genre>> {
  return this.http.post<Genre>(`${baseUrl}/Genre`, { name }).pipe(
    map((genre: Genre) => {
      return {
        success: true,
        data: genre
      } as ApiResponse<Genre>;
    }),
    catchError((error: HttpErrorResponse) => {
      if (error.status === 400) {
        const errorMessage = error.error?.errors?.['Custom Error']?.[0] || 'Error al crear el género';
        return of({
          success: false,
          errors: [errorMessage]
        } as ApiResponse<Genre>);
      }
      return of({
        success: false,
        errors: ['Error al crear el género']
      } as ApiResponse<Genre>);
    })
  );
}

updateGenre(id: number, name: string): Observable<ApiResponse> {
  return this.http.put(`${baseUrl}/Genre/${id}`, { name }).pipe(
    map(() => {
      return {
        success: true
      } as ApiResponse;
    }),
    catchError((error: HttpErrorResponse) => {
      if (error.status === 400) {
        const errorMessage = error.error?.errors?.['Custom Error']?.[0] || 'Error al actualizar el género';
        return of({
          success: false,
          errors: [errorMessage]
        } as ApiResponse);
      } else if (error.status === 404) {
        return of({
          success: false,
          errors: ['Género no encontrado']
        } as ApiResponse);
      }
      return of({
        success: false,
        errors: ['Error al actualizar el género']
      } as ApiResponse);
    })
  );
}

  toggleGenreActive(id: number): Observable<ApiResponse<{ currentStatus: boolean }>> {
    return this.http.patch<any>(
      `${baseUrl}/Genre/${id}/toggle-active`,
      {}
    ).pipe(
      map((response: any) => {
        // Mapear la respuesta del backend a la estructura ApiResponse
        return {
          success: true,
          data: {
            currentStatus: response.currentStatus
          },
          message: response.message
        } as ApiResponse<{ currentStatus: boolean }>;
      }),
      catchError((error: HttpErrorResponse) => {
        // Manejar errores y convertirlos a la estructura ApiResponse
        return of({
          success: false,
          errors: this.handleToggleError(error)
        } as ApiResponse<{ currentStatus: boolean }>);
      })
    );
  }

  private handleToggleError(error: HttpErrorResponse): string[] {
    if (error.status === 404) {
      return ['El género no fue encontrado'];
    } else if (error.status === 400) {
      return ['Solicitud inválida'];
    } else {
      return ['Error interno del servidor'];
    }
  }

  // Calificar un libro
  rateBook(bookId: number, rating: number): Observable<BookRatingResponseDto> {
    return this.http.post<BookRatingResponseDto>(`${baseUrl}/Book/${bookId}/rate`, {
      rating: rating
    });
  }

  // Obtener la calificación del usuario actual
  getUserRating(bookId: number): Observable<UserRatingDto> {
    return this.http.get<UserRatingDto>(`${baseUrl}/Book/${bookId}/user-rating`);
  }

  // Eliminar calificación
   deleteRating(bookId: number): Observable<any> {
    return this.http.delete(`${baseUrl}/Book/${bookId}/rating`);
  }

   uploadSubmissionCover(file: File): Observable<{url: string}> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{url: string}>(
      `${baseUrl}/BookSubmissions/cover`,
      formData
    );
  }

  deleteSubmissionCover(imageUrl: string): Observable<any> {
    return this.http.delete(`${baseUrl}/BookSubmissions/cover`, {
      params: { imageUrl }
    });
  }

  createBookSubmission(submission: CreateBookSubmissionDto): Observable<BookSubmissionResponseDto> {
    return this.http.post<BookSubmissionResponseDto>(
      `${baseUrl}/BookSubmissions`,
      submission
    );
  }

  // Obtener las submissions del usuario actual
  getMySubmissions(): Observable<BookSubmissionResponseDto[]> {
    return this.http.get<BookSubmissionResponseDto[]>(`${baseUrl}/BookSubmissions/my-submissions`);
  }

  // Obtener una submission específica
  getSubmission(id: number): Observable<BookSubmissionResponseDto> {
    return this.http.get<BookSubmissionResponseDto>(`${baseUrl}/BookSubmissions/${id}`);
  }

  // Eliminar una submission
  deleteSubmission(id: number): Observable<any> {
    return this.http.delete(`${baseUrl}/BookSubmissions/${id}`);
  }


  // Obtener todas las submissions (para admin)
  getAllSubmissions(): Observable<BookSubmissionResponseDto[]> {
    return this.http.get<BookSubmissionResponseDto[]>(`${baseUrl}/BookSubmissions/admin/all`);
  }

  // Obtener submissions pendientes (para admin)
  getPendingSubmissions(): Observable<BookSubmissionResponseDto[]> {
    return this.http.get<BookSubmissionResponseDto[]>(`${baseUrl}/BookSubmissions/admin/pending`);
  }

  getSubmissionStats(): Observable<SubmissionStatsDto> {
    return this.http.get<SubmissionStatsDto>(`${baseUrl}/BookSubmissions/admin/stats`)
    .pipe(
      tap( resp => console.log(resp)
      )
    );
  }

  // Actualizar estado de una submission (para admin)
  updateSubmissionStatus(id: number, updateDto: UpdateBookSubmissionStatusDto): Observable<any> {
    console.log(id);
    console.log(updateDto);

     const payload = {
      status: Number(updateDto.status),
      adminFeedback: updateDto.adminFeedback || null
    };

    console.log({payload});

    return this.http.put(`${baseUrl}/BookSubmissions/admin/${id}/status`, payload)
    .pipe(
      tap( resp => console.log(resp)
      )
    )
    ;
  }

  updateSubmission(id: number, updateDto: UpdateBookSubmissionDto): Observable<BookSubmissionResponseDto> {
    return this.http.put<BookSubmissionResponseDto>(
      `${baseUrl}/BookSubmissions/${id}`,
      updateDto
    );
  }

  updateBook(id: number, updateDto: UpdateBookDto): Observable<Book> {
    return this.http.put<Book>(`${baseUrl}/Book/${id}`, updateDto);
  }

  // Método para obtener un libro específico (si no lo tienes)
  getBookSimple(id: number): Observable<Book> {
    return this.http.get<Book>(`${baseUrl}/Book/${id}`);
  }

getVolumesByBook(bookId: number): Observable<VolumeSimpleDto[]> {
  return this.http.get<VolumeSimpleDto[]>(`${baseUrl}/Volume/book/${bookId}/volumes-simple`);
}

getChaptersByVolume(volumeId: number): Observable<ChapterDto[]> {
  return this.http.get<ChapterDto[]>(`${baseUrl}/Volume/${volumeId}/chapters`);
}

createVolume(volumeDto: VolumeCreateDto): Observable<any> {
  return this.http.post(`${baseUrl}/Volume/book/${volumeDto.bookId}/volumes`, volumeDto);
}

updateVolume(volumeId: number, volumeDto: VolumeUpdateDto): Observable<any> {
  return this.http.put(`${baseUrl}/Volume/${volumeId}`, volumeDto);
}

toggleVolumeActive(volumeId: number, isActive: boolean): Observable<{message: string, isActive: boolean}> {
  return this.http.patch<{message: string, isActive: boolean}>(
    `${baseUrl}/Volume/${volumeId}/toggle-active`,
    { isActive }
  );
}

  getChapterForAdmin(chapterId: number): Observable<ChapterAdminDto> {
    return this.http.get<ChapterAdminDto>(`${baseUrl}/Chapter/${chapterId}/admin`);
  }

  createChapter(chapterDto: CreateChapterDto): Observable<ChapterAdminDto> {
    return this.http.post<ChapterAdminDto>(`${baseUrl}/Chapter`, chapterDto);
  }

  updateChapter(chapterId: number, chapterDto: UpdateChapterDto): Observable<any> {
    return this.http.put(`${baseUrl}/Chapter/${chapterId}`, chapterDto);
  }

  toggleChapterActive(chapterId: number, isActive: boolean): Observable<{message: string, isActive: boolean}> {
    return this.http.patch<{message: string, isActive: boolean}>(
      `${baseUrl}/Chapter/${chapterId}/toggle-active`,
      { isActive }
    );
  }


}
