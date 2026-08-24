import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Vet {
  id: string;
  firstName: string;
  lastName: string;
  specialization: string;
  phone: string;
  email: string;
  clinicIds: string[];
  hasAccount?: boolean;
}

export interface CreateVetDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  specialization: string;
  clinicIds: string[];
}

/**
 * Parola temporară vine o singură dată, la crearea medicului, și trebuie
 * arătată imediat cabinetului.
 */
export interface VetAccount {
  vet: Vet;
  username: string;
  temporaryPassword: string;
  credentialsEmailed: boolean;
}

export interface VetClinic {
  id: string;
  name: string;
  address: string;
  phone: string;
}

export interface VetProfile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  specialization: string;
  clinics: VetClinic[];
  fullName: string;
}

export interface AppointmentPet {
  id: string;
  name: string;
  species: string;
  breed: string;
  age: number;
  weight: number;
  photoUrl?: string | null;
}

export interface AppointmentOwner {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  profilePhotoUrl?: string | null;
  fullName: string;
}

export interface AppointmentDetails {
  id: string;
  petId: string;
  vetId: string;
  clinicId: string;
  ownerId: string;
  dateTime: string;
  reason: string;
  status: number;
  pet?: AppointmentPet | null;
  owner?: AppointmentOwner | null;
}

/** Un pacient văzut din perspectiva medicului, pentru pagina de fișă medicală. */
export interface VetPatient {
  pet: AppointmentPet;
  owner?: AppointmentOwner | null;
  appointments: AppointmentDetails[];
  canWrite: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class VetService {
  private apiUrl = `${environment.apiUrl}/Vets`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Vet[]> {
    return this.http.get<Vet[]>(this.apiUrl);
  }

  getById(id: string): Observable<Vet> {
    return this.http.get<Vet>(`${this.apiUrl}/${id}`);
  }

  getByClinic(clinicId: string): Observable<Vet[]> {
    return this.http.get<Vet[]>(`${this.apiUrl}/by-clinic/${clinicId}`);
  }

  create(dto: CreateVetDto): Observable<VetAccount> {
    return this.http.post<VetAccount>(this.apiUrl, dto);
  }

  /** Creează contul unui medic adăugat înainte ca aplicația să genereze conturi. */
  createAccount(vetId: string, email: string): Observable<VetAccount> {
    return this.http.post<VetAccount>(`${this.apiUrl}/${vetId}/account`, { email });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /** Fișa medicului autentificat. */
  getMyProfile(): Observable<VetProfile> {
    return this.http.get<VetProfile>(`${this.apiUrl}/me`);
  }

  /** Agenda proprie a medicului autentificat. */
  getMyAppointments(): Observable<AppointmentDetails[]> {
    return this.http.get<AppointmentDetails[]>(`${this.apiUrl}/me/appointments`);
  }

  /** Un pacient al medicului autentificat, cu proprietar și programări. */
  getMyPatient(petId: string): Observable<VetPatient> {
    return this.http.get<VetPatient>(`${this.apiUrl}/me/patients/${petId}`);
  }
}
