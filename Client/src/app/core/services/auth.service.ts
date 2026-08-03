import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

export interface LoginModel {
  username: string;
  password: string;
}

export interface ClinicPrice {
  service: string;
  price: number;
}

export interface RegisterClinicPayload {
  name: string;
  street: string;
  streetNumber: string;
  city: string;
  county: string;
  phone: string;
  email: string;
  schedule: string;
  vetNames: string[];
  prices: ClinicPrice[];
}

export interface RegisterModel {
  username: string;
  email: string;
  password: string;
  role: string;
  clinic?: RegisterClinicPayload;
}

export interface AuthResponse {
  token: string;
  expiration: string;
  username: string;
  email: string;
  roles: string[];
  id: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  profilePhotoUrl?: string | null;
}

export interface UserProfile {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  phone: string;
  profilePhotoUrl?: string | null;
  fullName?: string;
}

export interface UpdateProfilePayload {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  profilePhotoUrl?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5149/api/Authenticate';
  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(null);
  currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    
    const stored = localStorage.getItem('currentUser');
    if (stored) {
      this.currentUserSubject.next(JSON.parse(stored));
    }
  }

  login(model: LoginModel): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, model).pipe(
      tap(response => {
        localStorage.setItem('currentUser', JSON.stringify(response));
        localStorage.setItem('token', response.token);
        this.currentUserSubject.next(response);
      })
    );
  }

  register(model: RegisterModel): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, model);
  }

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`).pipe(
      tap(profile => this.applyProfile(profile))
    );
  }

  updateProfile(payload: UpdateProfilePayload): Observable<UserProfile> {
    return this.http.put<UserProfile>(`${this.apiUrl}/profile`, payload).pipe(
      tap(profile => this.applyProfile(profile))
    );
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('token');
    this.currentUserSubject.next(null);
  }

  get currentUser(): AuthResponse | null {
    return this.currentUserSubject.value;
  }

  get isLoggedIn(): boolean {
    return !!this.currentUserSubject.value;
  }

  get token(): string | null {
    return localStorage.getItem('token');
  }

  hasRole(role: string): boolean {
    return this.currentUser?.roles?.includes(role) ?? false;
  }

  get isOwner(): boolean {
    return this.hasRole('Owner');
  }

  get isClinicAdmin(): boolean {
    return this.hasRole('ClinicAdmin');
  }

  get isSomoAdmin(): boolean {
    return this.hasRole('SomoAdmin');
  }

  get homeRoute(): string {
    if (this.isClinicAdmin) return '/clinic-dashboard';
    if (this.isSomoAdmin) return '/admin';
    return '/home';
  }

  get worksFromDashboard(): boolean {
    return this.isClinicAdmin || this.isSomoAdmin;
  }

  get displayName(): string {
    const user = this.currentUser;
    if (!user) return '';
    const fullName = [user.firstName, user.lastName].filter(n => !!n?.trim()).join(' ');
    return fullName || user.username;
  }

  private applyProfile(profile: UserProfile): void {
    const user = this.currentUser;
    if (!user) return;

    const updated: AuthResponse = {
      ...user,
      email: profile.email,
      firstName: profile.firstName,
      lastName: profile.lastName,
      phone: profile.phone,
      profilePhotoUrl: profile.profilePhotoUrl
    };
    localStorage.setItem('currentUser', JSON.stringify(updated));
    this.currentUserSubject.next(updated);
  }
}