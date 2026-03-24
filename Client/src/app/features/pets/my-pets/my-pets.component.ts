import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PetService, Pet } from '../../../core/services/pet.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-my-pets',
  templateUrl: './my-pets.component.html',
  styleUrls: ['./my-pets.component.css']
})
export class MyPetsComponent implements OnInit {
  pets: Pet[] = [];
  isLoading = true;
  showAddForm = false;
  isSubmitting = false;
  errorMessage = '';

  addPetForm: FormGroup;

  speciesList = ['Câine', 'Pisică', 'Iepure', 'Hamster', 'Papagal', 'Țestoasă', 'Reptilă', 'Alt animal'];

  constructor(
    private petService: PetService,
    private authService: AuthService,
    private fb: FormBuilder
  ) {
    this.addPetForm = this.fb.group({
      name: ['', Validators.required],
      species: ['Câine', Validators.required],
      breed: ['', Validators.required],
      age: [1, [Validators.required, Validators.min(0), Validators.max(50)]],
      weight: [1, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    this.loadPets();
  }

  loadPets(): void {
    const ownerId = this.authService.currentUser?.id || '';
    
    this.petService.getMyPets(ownerId).subscribe({
      next: pets => {
        this.pets = pets;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  onAddPet(): void {
    if (this.addPetForm.invalid) return;

    this.isSubmitting = true;
    this.errorMessage = '';

    this.petService.create(this.addPetForm.value).subscribe({
      next: pet => {
        this.pets.push(pet);
        this.addPetForm.reset({
          species: 'Câine',
          age: 1,
          weight: 1
        });
        this.showAddForm = false;
        this.isSubmitting = false;
      },
      error: () => {
        this.errorMessage = 'Eroare la adăugarea animalului.';
        this.isSubmitting = false;
      }
    });
  }

  onDeletePet(id: string): void {
    if (!confirm('Ești sigur că vrei să ștergi acest animal?')) return;

    this.petService.delete(id).subscribe({
      next: () => {
        this.pets = this.pets.filter(p => p.id !== id);
      }
    });
  }

  getSpeciesEmoji(species: string): string {
    const map: { [key: string]: string } = {
      'Câine': '🦮',
      'Pisică': '🐈‍⬛',
      'Iepure': '🐇',
      'Hamster': '🐁',
      'Papagal': '🦜',
      'Țestoasă': '🐢',
      'Reptilă': '🐍',
    };
    return map[species] || '🐾';
  }
}