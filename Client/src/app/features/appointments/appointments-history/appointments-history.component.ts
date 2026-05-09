import { Component, OnInit } from '@angular/core';
import { AppointmentService } from '../../../core/services/appointment.service';
import { PetService, Pet } from '../../../core/services/pet.service';
import { ClinicService, Clinic } from '../../../core/services/clinic.service';
import { VetService, Vet } from '../../../core/services/vet.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-appointments-history',
  templateUrl: './appointments-history.component.html',
  styleUrls: ['./appointments-history.component.css']
})
export class AppointmentsHistoryComponent implements OnInit {
  appointments: any[] = [];
  pets: Pet[] = [];
  clinics: Clinic[] = [];
  vets: Vet[] = [];
  isLoading = true;

  statusLabels: { [key: number]: string } = {
    0: 'În așteptare',
    1: 'Confirmat',
    2: 'Anulat',
    3: 'Finalizat'
  };

  statusColors: { [key: number]: string } = {
    0: '#f39c12',
    1: '#2ecc71',
    2: '#e74c3c',
    3: '#3498db'
  };

  constructor(
    private appointmentService: AppointmentService,
    private petService: PetService,
    private clinicService: ClinicService,
    private vetService: VetService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    const ownerId = this.authService.currentUser?.id || '';

    this.petService.getMyPets(ownerId).subscribe(pets => this.pets = pets);
    this.clinicService.getAll().subscribe(clinics => this.clinics = clinics);
    this.vetService.getAll().subscribe(vets => this.vets = vets);

    this.appointmentService.getMyAppointments(ownerId).subscribe({
      next: appointments => {
        this.appointments = appointments.sort((a, b) =>
          new Date(b.dateTime).getTime() - new Date(a.dateTime).getTime()
        );
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  getPetName(petId: string): string {
    return this.pets.find(p => p.id === petId)?.name || 'Animal necunoscut';
  }

  getClinicName(clinicId: string): string {
    return this.clinics.find(c => c.id === clinicId)?.name || 'Clinică necunoscută';
  }

  getVetName(vetId: string): string {
    const v = this.vets.find(v => v.id === vetId);
    return v ? `Dr. ${v.firstName} ${v.lastName}` : 'Medic necunoscut';
  }

  formatDate(dateTime: string): string {
    return new Date(dateTime).toLocaleDateString('ro-RO', {
      day: '2-digit', month: 'long', year: 'numeric'
    });
  }

  formatTime(dateTime: string): string {
    return new Date(dateTime).toLocaleTimeString('ro-RO', {
      hour: '2-digit', minute: '2-digit'
    });
  }

  canCancel(appointment: any): boolean {
    return appointment.status === 0 || appointment.status === 1;
  }

  onCancel(appointment: any): void {
    if (!confirm('Ești sigur că vrei să anulezi această programare?')) return;
    appointment.status = 2;
    
  }
}