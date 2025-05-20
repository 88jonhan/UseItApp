import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:7001/api';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private isLoggedInSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isLoggedIn$ = this.isLoggedInSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadUser();
  }

  private loadUser(): void {
    try {
      const token = localStorage.getItem('auth_token');
      const user = localStorage.getItem('user');

      if (token && user) {
        const parsedUser = JSON.parse(user);
        this.currentUserSubject.next(parsedUser);
        this.isLoggedInSubject.next(true);
      }
    } catch (e) {
      // Handle JSON parsing errors
      console.error('Error loading user from localStorage', e);
      this.logout(); // Clear potentially corrupted data
    }
  }

  private hasToken(): boolean {
    return !!localStorage.getItem('auth_token');
  }

  register(firstName: string, lastName: string, username: string, email: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/auth/register`, {
      firstName,
      lastName,
      username,
      email,
      password
    }).pipe(
      tap(response => this.handleAuthentication(response)),
      catchError(error => throwError(() => error.error?.message || 'Registration failed'))
    );
  }

  login(username: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/auth/login`, {
      username,
      password
    }).pipe(
      tap(response => this.handleAuthentication(response)),
      catchError(error => throwError(() => error.error?.message || 'Login failed'))
    );
  }

  private handleAuthentication(response: any): void {
    const { token, user } = response;

    localStorage.setItem('auth_token', token);
    localStorage.setItem('user', JSON.stringify(user));

    this.currentUserSubject.next(user);
    this.isLoggedInSubject.next(true);
  }

  logout(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('user');

    this.currentUserSubject.next(null);
    this.isLoggedInSubject.next(false);

    this.router.navigate(['/login']);
  }

  getCurrentUser(): Observable<User | null> {
    return this.currentUserSubject.asObservable();
  }

  getToken(): string | null {
    return localStorage.getItem('auth_token');
  }
}
