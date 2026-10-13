import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { catchError, map, Observable, of, tap } from 'rxjs';

import { User } from '../interfaces/user.interface';
import { AuthResponse } from '../interfaces/auth-response.interface';

const baseUrl = environment.baseUrl;

export interface RegisterResponse {
  success: boolean;
  data?: any;
  errors?: string[];
}


export interface ImageUploadResponse {
  success: boolean;
  url?: string;
  message?: string;
  errors?: string[];
}

type AuthStatus = 'checking' | 'authenticated' | 'not-authenticated';

@Injectable({providedIn: 'root'})
export class AuthService {

  constructor() {
  // Escuchar cambios en localStorage desde otras pestañas
  if (typeof window !== 'undefined') {
    window.addEventListener('storage', (event) => {
      // Si se elimina el token en otra pestaña → cerrar sesión aquí también
      if (event.key === 'token' && event.newValue === null) {
        this.logout();
      }
    });
  }
}

  private _authStatus = signal<AuthStatus>('checking');
  private _user = signal<User|null>(null);
  private _token = signal<string|null>(localStorage.getItem('token'));

  private http = inject(HttpClient);

  // getter para acceder a la propiedad privada
  authStatus = computed<AuthStatus>(() => { // computed -> solo lectura
    if (this._authStatus() === 'checking' ) {
      return 'checking';
    }

    if (this._user()) {
      return 'authenticated'
    }

    return 'not-authenticated';
  });

  user = computed<User|null>(() => this._user());
  token = computed(() => this._token());
  isAdmin = computed(() => this._user()?.roles.includes('Admin') ?? false);
  isAuthenticated = computed(() => this.authStatus() == 'authenticated' ? true : false );

  login(email: string, password:string):Observable<boolean|string> {
    return this.http.post<AuthResponse>(`${baseUrl}/Users/Login`, {
      email: email,
      password: password
    }).pipe(
      tap( resp => { //salio bien
        this.handleAuthSuccess(resp);
      }),
      map(() => true),
       catchError((error: HttpErrorResponse) => {
      if (error.status === 403) {
        // Si es error 403, devuelve el mensaje de error como string
        const errorMessage = 'Tu cuenta ha sido suspendida. Contacta al administrador';
        return of(errorMessage); // Devuelve el string
      }

      // Para otros errores, devuelve false
      this.handleAuthError(error);
      return of(false);
    })
    )
  }

  register(email: string, password: string, username: string):Observable<RegisterResponse> {
    return this.http.post<User>(`${baseUrl}/Users`, {
      username: username,
      email: email,
      password: password
    }).pipe(
      map((resp: any) => {
        // Registro exitoso (201)
        // console.log(resp);

        return {
          success: true,
          data: resp
        } as RegisterResponse;
      }),
      catchError((error: HttpErrorResponse) => {
        return of(this.handleRegisterError(error));
      })
    );
  }


  editUser(id: string, username?: string, newPassword?: string, confirmPassword?: string, currentPassword?: string):Observable<RegisterResponse> {
    return this.http.put<AuthResponse>(`${baseUrl}/Users/${id}`, {
      username,
      newPassword,
      confirmPassword,
      currentPassword
    }).pipe(
      tap( resp => {
        this.handleAuthSuccess(resp);
      }),
      map((resp) => {
        return {
          success: true,
          data: resp.user
        } as RegisterResponse;
      }),
      catchError((error: HttpErrorResponse) => {
        return of(this.handleRegisterError(error));
      })
    );
  }


  checkStatus(): Observable<boolean> {
    const token = localStorage.getItem('token');
    if (!token) {
      // this.logout();
      return of(false);
    }
    return this.http.get<AuthResponse>(`${baseUrl}/Users/check-status`, {
      // headers: {
      //   Authorization: `Bearer ${token}`
      // }
    }).pipe(
        tap( resp => { //salio bien
          this.handleAuthSuccess(resp);
        }),
        map(() => true),
        catchError((error : any) =>
          this.handleAuthError(error)
        )
    )

  }

  logout() {
    this._user.set(null);
    this._authStatus.set('not-authenticated');
    this._token.set(null);

    localStorage.removeItem('token');

    window.location.reload();
  }

  private handleAuthSuccess( {token, user}: AuthResponse ){
      this._user.set(user);
      this._authStatus.set('authenticated');
      this._token.set(token);

      localStorage.setItem('token', token);
  }

  private handleAuthError( error : any ) {
    this.logout();
    return of(false);
  }


  private handleRegisterError(error: HttpErrorResponse): RegisterResponse {
  const errorMessages: string[] = [];

  switch (error.status) {
    case 400:
      // Caso 1: Mensaje en propiedad 'message'
      if (error.error?.message) {
        errorMessages.push(error.error.message);
      }
      // Caso 2: Objeto con propiedad 'errors' (errores de validación del DTO)
      else if (error.error?.errors) {
        Object.keys(error.error.errors).forEach(field => {
          const fieldErrors = error.error.errors[field];
          errorMessages.push(...fieldErrors);
        });
      }
      // Caso 3: Otros objetos
      else if (typeof error.error === 'object' && error.error !== null) {
        Object.keys(error.error).forEach(key => {
          if (Array.isArray(error.error[key])) {
            errorMessages.push(...error.error[key]);
          } else if (typeof error.error[key] === 'string') {
            errorMessages.push(error.error[key]);
          }
        });
      }
      // Caso 4: Error genérico
      else {
        errorMessages.push('Error de validación en los datos enviados');
      }
      break;

    case 500:
      errorMessages.push('Error interno del servidor. Por favor, intenta más tarde.');
      break;

    default:
      errorMessages.push('Ha ocurrido un error inesperado. Por favor, intenta nuevamente.');
      break;
  }

  return {
    success: false,
    errors: errorMessages
  };
}

  uploadProfilePicture(file: File, targetUserId?: string): Observable<ImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);

    // Construir URL con query parameter opcional
    let url = `${baseUrl}/Users/profile-picture`;
    if (targetUserId) {
      url += `?id=${targetUserId}`;
    }

   return this.http.post<{url: string, message: string, userId: string}>(url, formData).pipe(
    tap(response => {
      if (!targetUserId || targetUserId === this.user()?.id) {
        const currentUser = this.user();

        if (currentUser) {
          this._user.set({
            ...currentUser,
            imageUrl: response.url
          });


        }
      }
    }),
    map(response => {
      return {
        success: true,
        url: response.url,
        message: response.message
      };
    }),
    catchError(error => {
      return of(this.handleImageUploadError(error));
    })
  );
  }


  deleteProfilePicture(targetUserId?: string): Observable<ImageUploadResponse> {
    const userId = targetUserId || this.user()?.id;

    if (!userId) {
      return of({
        success: false,
        errors: ['No se pudo identificar al usuario']
      });
    }

    return this.http.delete<{message: string}>(`${baseUrl}/Users/${userId}/profile`).pipe(
      tap(
        resp => console.log(resp),
      ),
      map(response => {
        // Actualizar el usuario local si es el usuario actual
        if (!targetUserId || targetUserId === this.user()?.id) {
          const currentUser = this.user();
          if (currentUser) {
            this._user.set({
              ...currentUser,
              imageUrl: null
            });
          }
        }

        return {
          success: true,
          message: response.message
        };
      }),
      catchError(error => {
        return of(this.handleImageUploadError(error));
      })
    );
  }

  private handleImageUploadError(error: HttpErrorResponse): ImageUploadResponse {
    const errorMessages: string[] = [];

    switch (error.status) {
      case 400:
        if (error.error?.Error) {
          errorMessages.push(error.error.Error);
        } else if (error.error?.message) {
          errorMessages.push(error.error.message);
        } else {
          errorMessages.push('Error en los datos enviados');
        }
        break;

      case 401:
        errorMessages.push('Debes iniciar sesión para realizar esta acción');
        break;

      case 403:
        errorMessages.push('No tienes permisos para realizar esta acción');
        break;

      case 404:
        errorMessages.push('Usuario no encontrado');
        break;

      case 413:
        errorMessages.push('La imagen es demasiado grande. Máximo 1MB permitido');
        break;

      case 415:
        errorMessages.push('Formato de imagen no soportado. Use JPG o PNG');
        break;

      case 500:
        errorMessages.push('Error interno del servidor. Intenta más tarde');
        break;

      default:
        errorMessages.push('Error inesperado al procesar la imagen');
        break;
    }

    return {
      success: false,
      errors: errorMessages
    };
  }
}
