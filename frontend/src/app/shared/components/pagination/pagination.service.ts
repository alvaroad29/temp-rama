import { inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PaginationService {

  private activatedRoute = inject(ActivatedRoute);

  currentPage = toSignal(
    this.activatedRoute.queryParamMap.pipe(
      map( params => (params.get('page') ? +params.get('page')! : 1)), // si viene page?="vacio" => x defecto 1
      map( page => (isNaN(page) ? 1 : page)), // x si no es un numero
      map( page => Math.max(1,page)) // valores negativos
    ),
    {
      initialValue: 1, // x si no viene ?page
    }
  )

}
