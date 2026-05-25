import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { AuthResponse } from '../core/models/user.model';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should login and store token', () => {
    const validJwt = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjMiLCJleHAiOjk5OTk5OTk5OTl9.fake';
    const mockResponse: AuthResponse = {
      token: validJwt,
      user: { id: '1', email: 'test@test.com', username: 'testuser', createdAt: new Date().toISOString() },
    };

    service.login('test@test.com', 'password').subscribe(res => {
      expect(res.token).toBe(validJwt);
      expect(service.getToken()).toBe(validJwt);
    });

    const req = httpMock.expectOne('/api/auth/login');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should register and store token', () => {
    const validJwt = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjMiLCJleHAiOjk5OTk5OTk5OTl9.fake';
    const mockResponse: AuthResponse = {
      token: validJwt,
      user: { id: '2', email: 'new@test.com', username: 'newuser', createdAt: new Date().toISOString() },
    };

    service.register('new@test.com', 'newuser', 'password').subscribe(res => {
      expect(res.token).toBe(validJwt);
      expect(service.isLoggedIn()).toBeTrue();
    });

    const req = httpMock.expectOne('/api/auth/register');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'new@test.com', username: 'newuser', password: 'password' });
    req.flush(mockResponse);
  });

  it('should logout and clear auth state', () => {
    localStorage.setItem('auth_token', 'some-token');
    localStorage.setItem('auth_user', JSON.stringify({ id: '1', email: 'a@b.com', username: 'u' }));

    service.logout();

    expect(service.getToken()).toBeNull();
    expect(localStorage.getItem('auth_token')).toBeNull();
  });

  it('should return false for isLoggedIn when no token', () => {
    localStorage.clear();
    const svc = TestBed.inject(AuthService);
    expect(svc.isLoggedIn()).toBeFalse();
  });
});
