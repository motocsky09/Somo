import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Vaccination {
  id: string;
  petId: string;
  vetId: string;
  clinicId: string;
  vaccineCode: string;
  vaccineName: string;
  batchNumber: string;
  notes: string;
  administeredOn: string;
  nextDueOn: string;
  reminderSent: boolean;
  vetName: string;
  clinicName: string;
  daysUntilDue: number;
}

export interface VaccineType {
  code: string;
  name: string;
  species: string;
  intervalMonths: number;
  isMandatory: boolean;
  description: string;
}

export interface CreateVaccinationDto {
  petId: string;
  vaccineCode: string;
  administeredOn: string;
  nextDueOn?: string | null;
  batchNumber: string;
  notes: string;
}

export interface UpdateVaccinationDto {
  vaccineCode: string;
  administeredOn: string;
  nextDueOn?: string | null;
  batchNumber: string;
  notes: string;
}

export type VaccinationStatus = 'overdue' | 'due-soon' | 'up-to-date';

@Injectable({
  providedIn: 'root'
})
export class VaccinationService {
  private apiUrl = `${environment.apiUrl}/Vaccinations`;

  /** Sub câte zile până la rapel considerăm vaccinul „aproape scadent”. */
  static readonly dueSoonThreshold = 30;

  constructor(private http: HttpClient) {}

  static statusOf(vaccination: Vaccination): VaccinationStatus {
    if (vaccination.daysUntilDue < 0) return 'overdue';
    if (vaccination.daysUntilDue <= VaccinationService.dueSoonThreshold) return 'due-soon';
    return 'up-to-date';
  }

  getCatalog(species?: string): Observable<VaccineType[]> {
    const query = species ? `?species=${encodeURIComponent(species)}` : '';
    return this.http.get<VaccineType[]>(`${this.apiUrl}/catalog${query}`);
  }

  getByPet(petId: string): Observable<Vaccination[]> {
    return this.http.get<Vaccination[]>(`${this.apiUrl}/pet/${petId}`);
  }

  create(dto: CreateVaccinationDto): Observable<Vaccination> {
    return this.http.post<Vaccination>(this.apiUrl, dto);
  }

  update(id: string, dto: UpdateVaccinationDto): Observable<Vaccination> {
    return this.http.put<Vaccination>(`${this.apiUrl}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
