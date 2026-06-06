import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Attaches the JWT bearer token to outgoing API requests and transparently
 * attempts a single refresh-and-retry when a 401 is encountered.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.getAccessToken();
  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      const isAuthCall = req.url.includes('/auth/');
      if (error.status === 401 && !isAuthCall && auth.getRefreshToken()) {
        return auth.refresh().pipe(
          switchMap((res) => {
            const retried = req.clone({
              setHeaders: { Authorization: `Bearer ${res.accessToken}` }
            });
            return next(retried);
          }),
          catchError((refreshError) => {
            auth.logout();
            void router.navigate(['/login']);
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
