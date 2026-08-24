import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface MedicalRecord {
  id: string;
  petId: string;
  vetId: string;
  clinicId: string;
  appointmentId: string;
  date: string;
  diagnosis: string;
  treatment: string;
  notes: string;
  weight: number;
  temperature: number;
  vetName: string;
  clinicName: string;
}

export interface CreateMedicalRecordDto {
  petId: string;
  appointmentId?: string;
  date?: string;
  diagnosis: string;
  treatment: string;
  notes: string;
  weight: number;
  temperature: number;
}

export interface UpdateMedicalRecordDto {
  date?: string;
  diagnosis: string;
  treatment: string;
  notes: string;
  weight: number;
  temperature: number;
}

@Injectable({
  providedIn: 'root'
})
export class MedicalRecordService {
  private apiUrl = `${environment.apiUrl}/MedicalRecords`;

  constructor(private http: HttpClient) {}

  getByPet(petId: string): Observable<MedicalRecord[]> {
    return this.http.get<MedicalRecord[]>(`${this.apiUrl}/pet/${petId}`);
  }

  create(dto: CreateMedicalRecordDto): Observable<MedicalRecord> {
    return this.http.post<MedicalRecord>(this.apiUrl, dto);
  }

  update(id: string, dto: UpdateMedicalRecordDto): Observable<MedicalRecord> {
    return this.http.put<MedicalRecord>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
