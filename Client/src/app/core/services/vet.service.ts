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
}