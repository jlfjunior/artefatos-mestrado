import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private readonly router: Router,
    private readonly toastr: ToastrService
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('access_token');

     const authReq = token ? req.clone({ headers: req.headers.set('Authorization', `Bearer ${token}`) }) : req;

     return next.handle(authReq).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.toastr.error('Sessão expirada. Faça login novamente.');
          localStorage.removeItem('access_token');
          this.router.navigate(['/login']);
        } else if (error.status === 403) {
          this.toastr.error('Você não tem permissão para acessar esse recurso.');
        }
        return throwError(() => error);
      })
    );
  }
}
