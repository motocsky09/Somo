import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppointmentService, AvailableSlot } from '../../../core/services/appointment.service';
import { PetService, Pet } from '../../../core/services/pet.service';
import { ClinicService, Clinic } from '../../../core/services/clinic.service';
import { VetService, Vet } from '../../../core/services/vet.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-create-appointment',
  templateUrl: './create-appointment.component.html',
  styleUrls: ['./create-appointment.component.css']
})
export class CreateAppointmentComponent implements OnInit {
  pets: Pet[] = [];
  clinics: Clinic[] = [];
  vets: Vet[] = [];
  filteredVets: Vet[] = [];
  availableSlots: AvailableSlot[] = [];

  selectedPetId = '';
  selectedClinicId = '';
  selectedVetId = '';
  selectedDate = '';
  selectedSlot = '';
  reason = '';

  isLoading = false;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  minDate = new Date().toISOString().split('T')[0];

  constructor(
    private appointmentService: AppointmentService,
    private petService: PetService,
    private clinicService: ClinicService,
    private vetService: VetService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const petId = this.route.snapshot.queryParams['petId'];
    if (petId) {
      this.selectedPetId = petId;
    }
    this.loadInitialData();
  }

  loadInitialData(): void {
    const ownerId = this.authService.currentUser?.id || '';
    this.petService.getMyPets(ownerId).subscribe(pets => this.pets = pets);
    this.clinicService.getAll().subscribe(clinics => this.clinics = clinics);
    this.vetService.getAll().subscribe(vets => this.vets = vets);
  }

  onClinicSelect(): void {
    this.selectedVetId = '';
    this.filteredVets = this.vets.filter(v =>
      v.clinicIds.includes(this.selectedClinicId)
    );
  }

  onVetOrDateChange(): void {
    if (this.selectedVetId && this.selectedDate) {
      this.loadSlots();
    }
  }

  loadSlots(): void {
    this.isLoading = true;
    this.availableSlots = [];
    this.selectedSlot = '';

    this.appointmentService.getAvailableSlots(this.selectedVetId, this.selectedDate)
      .subscribe({
        next: slots => {
          this.availableSlots = slots.filter(s => s.isAvailable);
          this.isLoading = false;
        },
        error: () => this.isLoading = false
      });
  }

  formatSlotTime(dateTime: string): string {
    return new Date(dateTime).toLocaleTimeString('ro-RO', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  canSubmit(): boolean {
    return !!(this.selectedPetId && this.selectedClinicId &&
              this.selectedVetId && this.selectedSlot && this.reason);
  }

  onSubmit(): void {
    if (!this.canSubmit()) return;

    this.isSubmitting = true;
    this.errorMessage = '';

    this.appointmentService.create({
      petId: this.selectedPetId,
      vetId: this.selectedVetId,
      clinicId: this.selectedClinicId,
      dateTime: this.selectedSlot,
      reason: this.reason
    }).subscribe({
      next: () => {
        this.successMessage = 'Programare creată cu succes!';
        setTimeout(() => this.router.navigate(['/my-pets']), 2000);
      },
      error: (err) => {
        this.errorMessage = err.error?.error || 'Eroare la crearea programării.';
        this.isSubmitting = false;
      }
    });
  }

  getPetName(id: string): string {
    return this.pets.find(p => p.id === id)?.name || '';
  }

  getClinicName(id: string): string {
    return this.clinics.find(c => c.id === id)?.name || '';
  }

  getVetName(id: string): string {
    const v = this.vets.find(v => v.id === id);
    return v ? `Dr. ${v.firstName} ${v.lastName}` : '';
  }
}