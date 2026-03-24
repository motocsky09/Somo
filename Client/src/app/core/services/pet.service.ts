import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Pet {
  id: string;
  name: string;
  species: string;
  breed: string;
  age: number;
  weight: number;
  ownerId: string;
}

export interface CreatePetDto {
  name: string;
  species: string;
  breed: string;
  age: number;
  weight: number;
}

@Injectable({
  providedIn: 'root'
})
export class PetService {
  private apiUrl = `${environment.apiUrl}/Pets`;

  constructor(private http: HttpClient) {}

  getMyPets(ownerId: string): Observable<Pet[]> {
    return this.http.get<Pet[]>(`${this.apiUrl}/owner/${ownerId}`);
  }

  create(pet: CreatePetDto): Observable<Pet> {
    return this.http.post<Pet>(this.apiUrl, pet);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}