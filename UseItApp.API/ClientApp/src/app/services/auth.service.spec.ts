import {TestBed} from '@angular/core/testing';
import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {HttpTestingController, provideHttpClientTesting} from '@angular/common/http/testing';
import {Router} from '@angular/router';
import {provideRouter} from '@angular/router';

import {AuthService} from './auth.service';
import {User} from '../models/user';
import {AppComponent} from '../app.component';

describe('AuthService', () => {
  let service: AuthService;
  let httpTestingController: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideRouter([]),
        provideHttpClientTesting(),
      ]
    }).compileComponents();

    service = TestBed.inject(AuthService);
    httpTestingController = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);

    // Mock router.navigate
    spyOn(router, 'navigate');
  });

  afterEach(() => {
    httpTestingController.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should start with not logged in state when localStorage is empty', () => {
    service.isLoggedIn$.subscribe(isLoggedIn => {
      expect(isLoggedIn).toBeFalse();
    });

    service.currentUser$.subscribe(user => {
      expect(user).toBeNull();
    });
  });

  it('should load user from localStorage on initialization', () => {
    const mockUser: User = {
      id: 1,
      username: 'testuser',
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User',
      isBlocked: false
    };

    // Viktigt: sätt data innan service skapas
    localStorage.setItem('auth_token', 'fake-jwt-token');
    localStorage.setItem('user', JSON.stringify(mockUser));

    // Återskapa testmodul & injicera service igen
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideRouter([]),
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
      ]
    });

    const authService = TestBed.inject(AuthService);

    authService.isLoggedIn$.subscribe(isLoggedIn => {
      expect(isLoggedIn).toBeTrue();
    });

    authService.currentUser$.subscribe(user => {
      expect(user).toEqual(mockUser);
    });
  });


  it('should register new user', () => {
    const mockResponse = {
      token: 'fake-jwt-token',
      user: {
        id: 1,
        username: 'newuser',
        email: 'new@example.com',
        firstName: 'New',
        lastName: 'User',
        isBlocked: false
      } as User
    };

    service.register('New', 'User', 'newuser', 'new@example.com', 'password123').subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpTestingController.expectOne('http://localhost:7001/api/auth/register');
    expect(req.request.method).toEqual('POST');

    // Ensure the request body matches what your backend expects
    expect(req.request.body).toEqual({
      firstName: 'New',
      lastName: 'User',
      username: 'newuser',
      email: 'new@example.com',
      password: 'password123'
    });

    req.flush(mockResponse);

    // Check localStorage was updated
    expect(localStorage.getItem('auth_token')).toEqual('fake-jwt-token');
    expect(localStorage.getItem('user')).toEqual(JSON.stringify(mockResponse.user));

    // Verify user state updated
    service.isLoggedIn$.subscribe(isLoggedIn => {
      expect(isLoggedIn).toBeTrue();
    });

    service.currentUser$.subscribe(user => {
      expect(user).toEqual(mockResponse.user);
    });
  });

  it('should login user with correct credentials', () => {
    const mockResponse = {
      token: 'fake-jwt-token',
      user: {
        id: 1,
        username: 'testuser',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
        isBlocked: false
      } as User
    };

    service.login('testuser', 'password123').subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    const req = httpTestingController.expectOne('http://localhost:7001/api/auth/login');
    expect(req.request.method).toEqual('POST');
    expect(req.request.body).toEqual({
      username: 'testuser',
      password: 'password123'
    });

    req.flush(mockResponse);

    // Check localStorage was updated
    expect(localStorage.getItem('auth_token')).toEqual('fake-jwt-token');
    expect(localStorage.getItem('user')).toEqual(JSON.stringify(mockResponse.user));
  });

  it('should handle login error', () => {
    const mockError = {
      error: {message: 'Invalid credentials'}
    };

    let errorResult: string | undefined;

    service.login('wronguser', 'wrongpass').subscribe({
      next: () => fail('Expected error, got success'),
      error: (error: string) => {
        errorResult = error;
      }
    });

    const req = httpTestingController.expectOne('http://localhost:7001/api/auth/login');
    req.flush(mockError.error, {status: 401, statusText: 'Unauthorized'});

    expect(errorResult).toBe('Invalid credentials');
    expect(localStorage.getItem('auth_token')).toBeNull();
  });

  it('should logout user', () => {
    // Setup authenticated state with minimal user object
    localStorage.setItem('auth_token', 'fake-jwt-token');
    localStorage.setItem('user', JSON.stringify({
      id: 1,
      username: 'testuser',
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User',
      isBlocked: false
    } as User));

    service.logout();

    // Check localStorage was cleared
    expect(localStorage.getItem('auth_token')).toBeNull();
    expect(localStorage.getItem('user')).toBeNull();

    // Check user state updated
    service.isLoggedIn$.subscribe(isLoggedIn => {
      expect(isLoggedIn).toBeFalse();
    });

    service.currentUser$.subscribe(user => {
      expect(user).toBeNull();
    });

    // Verify redirect to login
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });

  it('should get current user', () => {
    const mockUser: User = {
      id: 1,
      username: 'testuser',
      email: 'test@example.com',
      firstName: 'Test',
      lastName: 'User',
      isBlocked: false
    };

    localStorage.setItem('auth_token', 'fake-jwt-token');
    localStorage.setItem('user', JSON.stringify(mockUser));

    // Reload service to trigger init
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideRouter([]),
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(AuthService);

    service.getCurrentUser().subscribe(user => {
      expect(user).toEqual(mockUser);
    });
  });

  it('should return auth token', () => {
    localStorage.setItem('auth_token', 'fake-jwt-token');

    expect(service.getToken()).toEqual('fake-jwt-token');
  });
});
