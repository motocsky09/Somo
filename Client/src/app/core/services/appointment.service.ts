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

  getMyAppointments(ownerId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/owner/${ownerId}`);
  }
}