import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { delay, tap } from 'rxjs/operators';
import { User } from '../models/user.model';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { jwtDecode } from 'jwt-decode';
import { AuthToken } from '../models/auth.token.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;

  private readonly authServiceUrl = environment.keycloackRealmUrl;
  private readonly clientId = environment.keycloackClientId;
  private readonly grantType = environment.keycloackGrantType;

  constructor(private http: HttpClient) {
    let storedUser = null;
    if (typeof window !== 'undefined' && localStorage) {
      storedUser = localStorage.getItem('currentUser');
    }

    this.currentUserSubject = new BehaviorSubject<User | null>(
      storedUser ? JSON.parse(storedUser) : null
    );
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  login(username: string, password: string): Observable<User> {
    const body = new HttpParams()
      .set('client_id', this.clientId)
      .set('grant_type', this.grantType)
      .set('username', username)
      .set('password', password);

    const headers = new HttpHeaders().set('Content-Type', 'application/x-www-form-urlencoded');

    return this.http.post(this.authServiceUrl, body.toString(), { headers }).pipe(
      tap((res: any) => {
        localStorage.setItem('access_token', res.access_token);
        localStorage.setItem('refresh_token', res.refresh_token);

        let userData = this.getUserData(res.access_token);
        if (userData) {
            const user: User = {
            id: userData.id,
            email: userData.email,
            name: `${userData.given_name} ${userData.family_name}`,
            role: userData.realm_access?.roles.find(role => role === 'manager') || "",
            };

          localStorage.setItem('currentUser', JSON.stringify(user));
          this.currentUserSubject.next(user);
        } else {
          this.currentUserSubject.next(null);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return !!token;
  }

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  getUserData(token: string): AuthToken | null {
    if (!token) return null;

    const decoded: AuthToken = jwtDecode(token);
    return decoded;
  }
}