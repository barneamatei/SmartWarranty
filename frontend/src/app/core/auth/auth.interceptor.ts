import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';

import { ACCESS_TOKEN_KEY } from './auth-session.constants';
import { AuthViewModel } from '../../features/auth/view-models/auth.view-model';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authViewModel = inject(AuthViewModel);
  const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
  const isAuthSessionRequest =
    req.url.includes('/identity/api/Auth/login') ||
    req.url.includes('/identity/api/Auth/register') ||
    req.url.includes('/identity/api/Auth/refresh');

  const requestToSend =
    !accessToken || isAuthSessionRequest
      ? req
      : req.clone({
          setHeaders: {
            Authorization: `Bearer ${accessToken}`
          }
        });

  return next(requestToSend).pipe(
    catchError((error: unknown) => {
      if (
        !(error instanceof HttpErrorResponse) ||
        error.status !== 401 ||
        isAuthSessionRequest ||
        !authViewModel.hasRefreshToken()
      ) {
        return throwError(() => error);
      }

      return authViewModel.refreshSession().pipe(
        switchMap((token) =>
          next(
            req.clone({
              setHeaders: {
                Authorization: `Bearer ${token}`
              }
            })
          )
        ),
        catchError((refreshError) => {
          authViewModel.logoutLocally();
          return throwError(() => refreshError);
        })
      );
    })
  );
};
