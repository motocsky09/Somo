import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Clinic {
  id: string;
  name: string;
  address: string;
  city: string;
  phone: string;
  email: string;
  schedule: string;
  latitude: number;
  longitude: number;
  vetIds: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ClinicService {
  private apiUrl = `${environment.apiUrl}/Clinics`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Clinic[]> {
    return this.http.get<Clinic[]>(this.apiUrl);
  }

  getNearby(lat: number, lng: number, radiusKm: number = 10): Observable<Clinic[]> {
    return this.http.get<Clinic[]>(
      `${this.apiUrl}/nearby?lat=${lat}&lng=${lng}&radiusKm=${radiusKm}`
    );
  }

  getById(id: string): Observable<Clinic> {
    return this.http.get<Clinic>(`${this.apiUrl}/${id}`);
  }
}