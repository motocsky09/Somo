import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AvailableSlot {
  dateTime: string;
  isAvailable: boolean;
}

export interface CreateAppointmentDto {
  petId: string;
  vetId: string;
  clinicId: string;
  dateTime: string;
  reason: string;
}

export interface Appointment {
  id: string;
  petId: string;
  vetId: string;
  clinicId: string;
  dateTime: string;
  reason: string;
  status: number;
}

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {
  private apiUrl = `${environment.apiUrl}/Appointments`;

  constructor(private http: HttpClient) {}

  getAvailableSlots(vetId: string, date: string): Observable<AvailableSlot[]> {
    return this.http.get<AvailableSlot[]>(
      `${this.apiUrl}/available-slots?vetId=${vetId}&date=${date}`
    );
  }

  create(dto: CreateAppointmentDto): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }

  getMyAppointments(ownerId: string): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.apiUrl}/owner/${ownerId}`);
  }

  updateStatus(id: string, status: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, status);
  }
}