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
  photoUrl?: string | null;
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

  private static readonly speciesEmojis: { [species: string]: string } = {
    'Câine': '🦮',
    'Pisică': '🐈‍⬛',
    'Iepure': '🐇',
    'Hamster': '🐁',
    'Papagal': '🦜',
    'Țestoasă': '🐢',
    'Reptilă': '🐍'
  };

  static speciesEmoji(species: string): string {
    return PetService.speciesEmojis[species] || '🐾';
  }

  constructor(private http: HttpClient) {}

  getMyPets(ownerId: string): Observable<Pet[]> {
    return this.http.get<Pet[]>(`${this.apiUrl}/owner/${ownerId}`);
  }

  getById(id: string): Observable<Pet> {
    return this.http.get<Pet>(`${this.apiUrl}/${id}`);
  }

  create(pet: CreatePetDto): Observable<Pet> {
    return this.http.post<Pet>(this.apiUrl, pet);
  }

  update(id: string, pet: Pet): Observable<Pet> {
    return this.http.put<Pet>(`${this.apiUrl}/${id}`, pet);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}