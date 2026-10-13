import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Announcement } from '../../library-front/interfaces/announcement-interface';
import { catchError, map, Observable, of, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';

const baseUrl = environment.baseUrl;

export interface ApiResponse {
  success: boolean;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AnnoucementsService {

  private http = inject(HttpClient);

  getAnnoucements(): Observable<Announcement[]> {
    return this.http.get<Announcement[]>(`${baseUrl}/Announcements`);
  }

  getLatestAnnoucement(): Observable<Announcement | null> {
      return this.http.get<Announcement|null>(`${baseUrl}/Announcements/latest`).pipe(
        catchError(error => {

          // error 404 (No hay anuncios), devolver null
          if (error.status === 404) {
            return of(null);
          }

          // otros errores, lanzar excepción
          let errorMessages: string[] = [];

          if (error.error && error.error.errors && Array.isArray(error.error.errors)) {
            errorMessages = error.error.errors;
          } else if (error.error && error.error.message) {
            errorMessages = [error.error.message];
          } else {
            errorMessages = ['Error al obtener el último anuncio'];
          }

          return throwError(() => ({
            success: false,
            errors: errorMessages
          }));
        })
      );
    }

  getAnnoucementById(id: number): Observable<Announcement>  {
    return this.http.get<Announcement>(`${baseUrl}/Announcements/${id}`).pipe(
      catchError(error => {
        console.error('Error al obtener anuncio:', error);

        let errorMessage = 'Error al cargar el anuncio';

        if (error.status === 404) {
          errorMessage = 'El anuncio no fue encontrado';
        } else if (error.status === 403) {
          errorMessage = 'No tienes permisos para ver este anuncio';
        } else if (error.status === 401) {
          errorMessage = 'No autorizado';
        } else if (error.error?.message) {
          errorMessage = error.error.message;
        } else if (error.error?.errors && Array.isArray(error.error.errors)) {
          errorMessage = error.error.errors.join(', ');
        }

        return throwError(() => new Error(errorMessage));
      })
    );
  }

  deleteAnnoucement(id: number): Observable<ApiResponse>  {
   return this.http.delete(`${baseUrl}/Announcements/${id}`).pipe(
      // Si la eliminación es exitosa (204 No Content, 200 OK, etc.), devolvemos éxito
      map(() => ({
        success: true
      })),
      // Capturamos cualquier error HTTP (400, 404, 500, etc.)
      catchError(error => {
        console.error('Error al eliminar anuncio:', error);

        // Extraemos los mensajes de error del cuerpo de la respuesta, si existen
        // Asumiendo que tu backend devuelve errores en un formato como:
        // { "errors": ["Mensaje 1", "Mensaje 2"] } o { "message": "Mensaje único" }
        let errorMessages: string[] = [];

        if (error.error && error.error.errors && Array.isArray(error.error.errors)) {
          errorMessages = error.error.errors;
        } else if (error.error && error.error.message) {
          errorMessages = [error.error.message];
        } else {
          // Mensaje genérico si no hay detalles
          errorMessages = ['Ocurrió un error al eliminar el anuncio.'];
        }

        // Devolvemos un objeto ApiResponse indicando fallo y los mensajes de error
        return throwError(() => ({
          success: false,
          errors: errorMessages
        }));
      })
    );
  }

   editAnnoucement(id: number, title: string, content: string, date: boolean): Observable<ApiResponse>  {
   return this.http.put(`${baseUrl}/Announcements/${id}`, {
    title,
    content,
    UpdateDate: date
   }).pipe(
      map(() => ({
        success: true
      })),
      catchError(error => {

        let errorMessages: string[] = [];

        if (error.error && error.error.errors && Array.isArray(error.error.errors)) {
          errorMessages = error.error.errors;
        } else if (error.error && error.error.message) {
          errorMessages = [error.error.message];
        } else {
          errorMessages = ['Ocurrió un error al eliminar el anuncio.'];
        }

        return throwError(() => ({
          success: false,
          errors: errorMessages
        }));
      })
    );
  }

  createAnnoucement(title:string, content: string): Observable<ApiResponse>  {
   return this.http.post(`${baseUrl}/Announcements`, {
    title: title,
    content: content,
   }).pipe(
      map(() => ({
        success: true
      })),

      catchError(error => {

        let errorMessages: string[] = [];

        if (error.error && error.error.errors && Array.isArray(error.error.errors)) {
          errorMessages = error.error.errors;
        } else if (error.error && error.error.message) {
          errorMessages = [error.error.message];
        } else {
          // Mensaje genérico si no hay detalles
          errorMessages = ['Ocurrió un error al eliminar el anuncio.'];
        }

        return throwError(() => ({
          success: false,
          errors: errorMessages
        }));
      })
    );
  }

}
