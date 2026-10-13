import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map, Observable, of, switchMap, tap, throwError } from 'rxjs';
import { List, ListsResponse } from '../interfaces/list-response.interface';
import { environment } from '../../../../environments/environment';
import { BookListQueryParameters, SortByBookList } from '../interfaces/book-list-books-query-parameters.interface';
import { BookList, BooksListDetailedResponse } from '../interfaces/list-detailed.interface';
import { BookListWithMembershipDto } from '../interfaces/book-list-membership';

const baseUrl = environment.baseUrl;

export interface ApiResponse {
  success: boolean;
  message?: string;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ListsService {

  private http = inject(HttpClient);

  getLists(options: BookListQueryParameters): Observable<ListsResponse> {
      const defaultedOptions = {
        PageNumber: 1,
        PageSize: 10,
        SortDescending: false,
        SortBy: SortByBookList.Title,
        ...options
      };

      // Filtrar propiedades no deseadas
      const filteredOptions = Object.fromEntries(
        Object.entries(defaultedOptions)
          .filter(([_, value]) => {
            // Eliminar undefined, null, string vacío
            if (value === undefined || value === null || value === '') return false;

            // Eliminar arrays vacíos
            if (Array.isArray(value) && value.length === 0) return false;

            return true;
          })
      );

      // Crear HttpParams manualmente para manejar arrays correctamente
      let params = new HttpParams();

      Object.entries(filteredOptions).forEach(([key, value]) => {
        if (Array.isArray(value)) {
          // Para arrays, agregar cada valor individualmente
          value.forEach(item => {
            params = params.append(key, item.toString());
          });
        } else {
          // Para valores simples
          params = params.set(key, value.toString());
        }
      });

      console.log('Params enviados:', params.toString());
      return this.http.get<ListsResponse>(`${baseUrl}/BookLists`, { params });
    }


  getDetailedList(id: number,options: BookListQueryParameters): Observable<BooksListDetailedResponse> {
    const {
      PageNumber = 1,
      PageSize = 20,
      SortDescending = false,
      SortBy = SortByBookList.Title} = {
    } = options;

    return this.http.get<BooksListDetailedResponse>(`${baseUrl}/BookLists/${id}/detailed`, {
      params: {
        PageNumber,
        PageSize,
        SortDescending,
        SortBy
      }
    });
  }

  getListsByUser(id: string): Observable<List[]> {
    return this.http.get<List[]>(`${baseUrl}/BookLists/user/${id}/`);
  }

  getUserBookListsWithBookMembership(userId: string, bookId: number): Observable<BookListWithMembershipDto[]> {
    return this.http.get<BookListWithMembershipDto[]>(`${baseUrl}/BookLists/user/${userId}/book/${bookId}`);
  }

  addBookToList(bookListId: number, bookId: number): Observable<any> {
    return this.http.post(`${baseUrl}/BookLists/${bookListId}/books/${bookId}`, {});
  }

  removeBookFromList(bookListId: number, bookId: number): Observable<any> {
    return this.http.delete(`${baseUrl}/BookLists/${bookListId}/books/${bookId}`);
  }

  getListById(id: number): Observable<List> {
    // console.log("pedir lista");
    // console.log(id);
    return this.http.get<List>(`${baseUrl}/BookLists/${id}`).pipe(
        // tap(resp => console.log(resp))
      )
    ;
  }

  deleteList(listId: number): Observable<ApiResponse> {
    return this.http.delete(`${baseUrl}/BookLists/${listId}`, { observe: 'response' }).pipe(
      map(response => {
        // Para respuestas 204 No Content, creamos una respuesta exitosa
        if (response.status === 204) {
          return {
            success: true,
            message: 'Lista eliminada correctamente'
          };
        }

        // Para otras respuestas exitosas
        return {
          success: true,
          message: 'Lista eliminada correctamente',
          data: response.body
        };
      }),
      catchError(error => {
        console.error('Error eliminando lista:', error);

        let errorMessage = 'Error al eliminar la lista';

        // Manejar diferentes códigos de error
        if (error.status === 401) {
          errorMessage = 'Debes iniciar sesión para eliminar la lista.';
        } else if (error.status === 403) {
          errorMessage = 'No tienes permisos para eliminar esta lista.';
        } else if (error.status === 404) {
          errorMessage = 'La lista no fue encontrada.';
        } else if (error.status === 400) {
          errorMessage = error.error?.message || 'Solicitud inválida.';
        } else if (error.status >= 500) {
          errorMessage = 'Error del servidor. Intenta nuevamente más tarde.';
        }

        return throwError(() => ({
          success: false,
          errors: [errorMessage],
          status: error.status
        }));
      })
    );
  }


  createList(
    title: string,
    description: string,
    isPublic: boolean,
    genreIds: number[],
    imageFile?: File | null
  ): Observable<ApiResponse> {

    // Si hay imagen, subirla primero
    if (imageFile) {
      return this.uploadImage(imageFile).pipe(
        switchMap(imageUrl => {
          // Si la subida fue exitosa, crear la lista con la URL de la imagen
          return this.createListWithData(title, description, isPublic, genreIds, imageUrl);
        }),
        catchError(uploadError => {
          // Si falla la subida de imagen, crear la lista sin imagen
          console.warn('Error subiendo imagen, creando lista sin imagen:', uploadError);
          return this.createListWithData(title, description, isPublic, genreIds, null);
        })
      );
    } else {
      // Si no hay imagen, crear la lista directamente
      return this.createListWithData(title, description, isPublic, genreIds, null);
    }
  }



  private createListWithData(
    title: string,
    description: string,
    isPublic: boolean,
    genreIds: number[],
    imageUrl: string | null
  ): Observable<ApiResponse> {

    const requestBody = {
      title,
      description: description || null,
      imageUrl: imageUrl,
      isPublic,
      genreIds
    };

    console.log('Datos enviados al servidor:', requestBody);

    return this.http.post<ApiResponse>(`${baseUrl}/BookLists`, {
      title,
      description: description || null,
      imageUrl: imageUrl, // Usar la URL de la imagen o null
      isPublic,
      genreIds
    }).pipe(
      map(apiResponse => {
        return {
          success: true,
          message: 'Lista creada exitosamente'
        };
      }),
      catchError(error => {
        console.error('Error completo del servidor:', error);

        let errorMessage = 'Error al crear la lista';

        if (error.status === 400) {
          errorMessage = 'Datos inválidos. Verifica la información ingresada.';
          if (error.error?.message) {
            errorMessage = error.error.message;
          }
        } else if (error.status === 401) {
          errorMessage = 'Debes iniciar sesión para crear una lista.';
        } else if (error.status === 403) {
          errorMessage = 'No tienes permisos para realizar esta acción.';
        } else if (error.status >= 500) {
          errorMessage = 'Error del servidor. Intenta nuevamente más tarde.';
        }

        return throwError(() => ({
          success: false,
          errors: [errorMessage]
        }));
      })
    );
  }

updateList(
  listId: number,
  title: string,
  description: string,
  isPublic: boolean,
  genreIds: number[],
  imageFile?: File | null,
  removeImage: boolean = false
): Observable<ApiResponse> {

  // CASO 1: El usuario quiere eliminar la imagen existente
  if (removeImage) {
    return this.deleteListImage(listId).pipe(
      switchMap(() => {
        // Después de eliminar la imagen, actualizar el resto de los datos
        return this.updateListWithData(listId, title, description, isPublic, genreIds, null);
      }),
      catchError(deleteError => {
        // Si falla la eliminación, continuar con la actualización sin cambiar la imagen
        console.warn('Error eliminando imagen, actualizando lista sin cambiar imagen:', deleteError);
        return this.updateListWithData(listId, title, description, isPublic, genreIds, null);
      })
    );
  }

  // CASO 2: El usuario quiere subir una nueva imagen
  if (imageFile) {
    return this.uploadImage(imageFile, listId).pipe(
      switchMap(imageUrl => {
        return this.updateListWithData(listId, title, description, isPublic, genreIds, imageUrl);
      }),
      catchError(uploadError => {
        console.warn('Error subiendo imagen, actualizando lista sin cambiar imagen:', uploadError);
        return this.updateListWithData(listId, title, description, isPublic, genreIds, null);
      })
    );
  }

  // CASO 3: No hay cambios en la imagen, actualizar solo los datos
  return this.updateListWithData(listId, title, description, isPublic, genreIds, null);
}

// Método privado actualizado para updateListWithData
private updateListWithData(
  listId: number,
  title: string,
  description: string,
  isPublic: boolean,
  genreIds: number[],
  imageUrl: string | null // null significa "no cambiar la imagen actual"
): Observable<ApiResponse> {

  const updateData: any = {
    title,
    description: description || null,
    isPublic,
    genreIds
  };

  // Solo incluir imageUrl si no es null (para no cambiar la imagen existente)
  if (imageUrl !== null) {
    updateData.imageUrl = imageUrl;
  }

  console.log('Actualizando lista con datos:', updateData);

  return this.http.put<ApiResponse>(`${baseUrl}/BookLists/${listId}`, updateData).pipe(
    map(() => {
      return {
        success: true,
        message: 'Lista actualizada exitosamente'
      };
    }),
    catchError(error => {
      console.error('Error completo del servidor:', error);

      let errorMessage = 'Error al actualizar la lista';
      if (error.status === 400) {
        errorMessage = error.error?.message || 'Datos inválidos. Verifica la información ingresada.';
      } else if (error.status === 401) {
        errorMessage = 'Debes iniciar sesión para actualizar la lista.';
      } else if (error.status === 403) {
        errorMessage = 'No tienes permisos para realizar esta acción.';
      } else if (error.status === 404) {
        errorMessage = 'La lista no fue encontrada.';
      } else if (error.status >= 500) {
        errorMessage = 'Error del servidor. Intenta nuevamente más tarde.';
      }

      return throwError(() => ({
        success: false,
        errors: [errorMessage]
      }));
    })
    )
  }

 deleteListImage(listId: number): Observable<ApiResponse> {
  return this.http.delete<ApiResponse>(`${baseUrl}/BookLists/${listId}/cover`).pipe(
    map(() => {
      return {
        success: true,
        message: 'Imagen eliminada correctamente'
      };
    }),
    catchError(error => {
      console.error('Error eliminando imagen:', error);

      let errorMessage = 'Error al eliminar la imagen';
      if (error.status === 400) {
        errorMessage = error.error?.Error || 'La lista no tiene imagen para eliminar';
      } else if (error.status === 401) {
        errorMessage = 'Debes iniciar sesión para realizar esta acción.';
      } else if (error.status === 403) {
        errorMessage = 'No tienes permisos para realizar esta acción.';
      } else if (error.status === 404) {
        errorMessage = 'Lista no encontrada.';
      } else if (error.status >= 500) {
        errorMessage = 'Error del servidor. Intenta nuevamente más tarde.';
      }

      return throwError(() => ({
        success: false,
        errors: [errorMessage]
      }));
    })
  );
}
  uploadImage(imageFile: File, listId?: number): Observable<string>
  {
    const formData = new FormData();
    formData.append('file', imageFile);

    let url = `${baseUrl}/BookLists/cover`;

    if (listId) {
      url += `?listId=${listId}`;
    }

    return this.http.post<{url: string}>(url, formData).pipe(
      map(resp => {

        return resp.url;
      }),
      catchError(error => {


        let errorMessage = 'Error al subir la imagen';
        if (error.status === 413) {
          errorMessage = 'La imagen es demasiado grande. Máximo 2MB.';
        } else if (error.status === 415) {
          errorMessage = 'Formato de imagen no soportado. Use JPG, PNG o WebP.';
        }

        return throwError(() => errorMessage);
      })
    );
  }

  // manejo lista del carousel

 getOrCreateCarouselList(): Observable<any> {
  console.log("getOrCreateCarouselList");

  return this.getCarouselList().pipe(
    switchMap(resp => {
      // Verificar si encontró la lista (resp.data tiene al menos un elemento)
      if (resp.data && resp.data.length > 0) {
        console.log("Lista del carousel encontrada:", resp.data[0]);
        // Devolver el primer resultado (que debería ser "Home Carousel")
        return of(resp.data[0]);
      } else {
        console.log("Lista del carousel no encontrada, creando...");
        // Si no existe, crear la lista del carousel
        return this.createCarouselList();
      }
    }),
    catchError(error => {
      console.log("getOrCreateCarouselList error", error);
      // Si hay un error de red u otro, intentar crear la lista
      return this.createCarouselList();
    })
  );
}

  // Buscar la lista del carousel por título o identificador especial
  private getCarouselList(): Observable<ListsResponse> {
    console.log("getCarouselList");
    return this.http.get<ListsResponse>(`${baseUrl}/BookLists`, {
      params: {
        SearchText: 'Home Carousel',
        PageSize: 1
      }
    });
  }

  // Crear la lista especial del carousel
  private createCarouselList(): Observable<any> {
    return this.createListWithData(
      'Home Carousel',
      'Libros destacados para el carousel de la página principal',
      true, // isPublic
      [],   // genreIds - vacío o los que quieras
      null  // imageUrl
    ).pipe(
      map(response => response)
    );
  }

  // Obtener los libros del carousel para el home
  getCarouselBooks(): Observable<BookList[]> {
    return this.getCarouselList().pipe(
      switchMap(carouselList =>
        this.getDetailedList(carouselList.data[0].id, {
          PageNumber: 1,
          PageSize: 10, // o el número que quieras mostrar
          SortBy: SortByBookList.Title,
          SortDescending: false
        })
      ),
      map(detailedResponse => detailedResponse.books.data),
      catchError(error => {
        console.error('Error obteniendo libros del carousel:', error);
        return of([]); // Retornar array vacío en caso de error
      })
    );
  }


  // seguimiento de listas
  followList(bookListId: number): Observable<any> {
    return this.http.post(`${baseUrl}/BookLists/follow`, { bookListId });
  }

  unfollowList(bookListId: number): Observable<any> {
    return this.http.post(`${baseUrl}/BookLists/unfollow`, { bookListId });
  }

  checkIfFollowing(bookListId: number): Observable<boolean> {
    return this.http.get<boolean>(`${baseUrl}/BookLists/${bookListId}/is-following`);
  }

  getFollowedLists(options: BookListQueryParameters): Observable<ListsResponse> {
    const defaultedOptions = {
      PageNumber: 1,
      PageSize: 10,
      SortDescending: false,
      SortBy: SortByBookList.Title,
      ...options
    };

    // Filtrar propiedades no deseadas
    const filteredOptions = Object.fromEntries(
      Object.entries(defaultedOptions)
        .filter(([_, value]) => {
          if (value === undefined || value === null || value === '') return false;
          if (Array.isArray(value) && value.length === 0) return false;
          return true;
        })
    );

    // Crear HttpParams manualmente
    let params = new HttpParams();
    Object.entries(filteredOptions).forEach(([key, value]) => {
      if (Array.isArray(value)) {
        value.forEach(item => {
          params = params.append(key, item.toString());
        });
      } else {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<ListsResponse>(`${baseUrl}/BookLists/followed`, { params });
  }



}
