import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ClinicPrice } from './auth.service';

export type ClinicStatus = 'Pending' | 'Approved' | 'Rejected';

export interface AdminOverview {
  owners: number;
  clinicAdmins: number;
  clinicsTotal: number;
  clinicsPending: number;
  clinicsApproved: number;
  clinicsRejected: number;
}

export interface AdminOwnerPet {
  id: string;
  name: string;
  species: string;
  breed: string;
  age: number;
}

export interface AdminOwner {
  id: string;
  username: string;
  email: string;
  pets: AdminOwnerPet[];
}

export interface AdminClinic {
  id: string;
  name: string;
  address: string;
  street: string;
  streetNumber: string;
  city: string;
  county: string;
  phone: string;
  email: string;
  schedule: string;
  vetNames: string[];
  prices: ClinicPrice[];
  status: ClinicStatus;
  rejectionReason: string;
  requestedAtUtc: string;
  reviewedAtUtc: string | null;
  adminId: string;
  adminUsername: string | null;
  adminEmail: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = `${environment.apiUrl}/Admin`;

  constructor(private http: HttpClient) {}

  getOverview(): Observable<AdminOverview> {
    return this.http.get<AdminOverview>(`${this.apiUrl}/overview`);
  }

  getOwners(): Observable<AdminOwner[]> {
    return this.http.get<AdminOwner[]>(`${this.apiUrl}/owners`);
  }

  getClinics(): Observable<AdminClinic[]> {
    return this.http.get<AdminClinic[]>(`${this.apiUrl}/clinics`);
  }

  approveClinic(id: string): Observable<AdminClinic> {
    return this.http.post<AdminClinic>(`${this.apiUrl}/clinics/${id}/approve`, {});
  }

  rejectClinic(id: string, reason: string): Observable<AdminClinic> {
    return this.http.post<AdminClinic>(`${this.apiUrl}/clinics/${id}/reject`, { reason });
  }

  deleteClinic(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/clinics/${id}`);
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/users/${id}`);
  }
}
