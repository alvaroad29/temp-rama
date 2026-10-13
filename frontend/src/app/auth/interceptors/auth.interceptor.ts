import { HttpRequest, HttpHandlerFn, HttpEvent, HttpEventType } from "@angular/common/http";
import { AuthService } from "../services/auth.service";
import { inject } from "@angular/core";

export function authInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) {

  const token = inject(AuthService).token(); // token (señal)

  const newReq = req.clone({
    headers: req.headers.append( 'Authorization', `Bearer ${token}`),
  });
  return next(newReq);
}
