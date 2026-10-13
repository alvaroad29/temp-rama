import { ApplicationConfig, inject, provideAppInitializer, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { AuthService } from './auth/services/auth.service';
import { authInterceptor } from './auth/interceptors/auth.interceptor';
import { provideMarkdown, MARKED_OPTIONS } from 'ngx-markdown';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withFetch(),
      withInterceptors([
        authInterceptor
      ]),
    ),
    provideAppInitializer(() => inject(AuthService).checkStatus()),
     provideMarkdown({
      markedOptions: {
        provide: MARKED_OPTIONS,
        useValue: { breaks: true, gfm: true },
      },
  }),
  ]
};
