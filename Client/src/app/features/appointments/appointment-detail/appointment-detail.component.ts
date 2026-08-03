import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-appointment-detail',
  templateUrl: './appointment-detail.component.html',
  styleUrls: ['./appointment-detail.component.css']
})
export class AppointmentDetailComponent implements OnInit {
  appointment: any = null;
  pet: any = null;
  vet: any = null;
  owner: any = null;
  isLoading = true;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  isEditingDate = false;
  isEditingVet = false;
  clinicVets: any[] = [];
  newDateTime = '';
  newVetId = '';

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
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) { this.router.navigate(['/clinic-dashboard']); return; }

    this.http.get<any>(`${environment.apiUrl}/Appointments/${id}/details`).subscribe({
      next: (app) => {
        this.appointment = app;
        this.newDateTime = new Date(app.dateTime).toISOString().slice(0, 16);
        this.newVetId = app.vetId;
        this.pet = app.pet;
        this.vet = app.vet;
        this.owner = app.owner;

        this.http.get<any[]>(`${environment.apiUrl}/Vets`).subscribe({
          next: (allVets) => {
            this.clinicVets = allVets.filter(v => v.clinicIds.includes(app.clinicId));
            this.isLoading = false;
          },
          error: () => { this.isLoading = false; }
        });
      },
      error: () => { this.router.navigate(['/clinic-dashboard']); }
    });
  }

  updateStatus(status: number): void {
    if (status === 2 && !confirm('Ești sigur că vrei să anulezi această programare?')) return;

    this.isSaving = true;
    this.http.patch(`${environment.apiUrl}/Appointments/${this.appointment.id}/status`, status).subscribe({
      next: () => {
        this.appointment.status = status;
        this.isSaving = false;
        this.successMessage = 'Status actualizat cu succes!';
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: () => { this.isSaving = false; this.errorMessage = 'Eroare la actualizare.'; }
    });
  }

  saveChanges(): void {
    this.isSaving = true;
    this.errorMessage = '';
    const updated = {
      id: this.appointment.id,
      petId: this.appointment.petId,
      ownerId: this.appointment.ownerId,
      clinicId: this.appointment.clinicId,
      reason: this.appointment.reason,
      status: this.appointment.status,
      dateTime: new Date(this.newDateTime).toISOString(),
      vetId: this.newVetId
    };

    this.http.put(`${environment.apiUrl}/Appointments/${this.appointment.id}`, updated).subscribe({
      next: () => {
        this.appointment.dateTime = updated.dateTime;
        this.appointment.vetId = updated.vetId;
        this.vet = this.clinicVets.find(v => v.id === this.newVetId);
        this.isEditingDate = false;
        this.isEditingVet = false;
        this.isSaving = false;
        this.successMessage = 'Programare actualizată!';
        setTimeout(() => this.successMessage = '', 3000);
      },
      error: () => { this.isSaving = false; this.errorMessage = 'Eroare la salvare.'; }
    });
  }

  goBack(): void {
    this.router.navigate(['/clinic-dashboard']);
  }

  isPast(): boolean {
    return new Date(this.appointment?.dateTime) < new Date();
  }

  get ownerName(): string {
    if (!this.owner) return '';
    const fullName = [this.owner.firstName, this.owner.lastName].filter((n: string) => !!n).join(' ');
    return fullName || this.owner.username;
  }

  getSpeciesEmoji(species: string): string {
    const map: { [key: string]: string } = {
      'Câine': '🦮', 'Pisică': '🐈‍⬛', 'Iepure': '🐇',
      'Hamster': '🐁', 'Papagal': '🦜', 'Țestoasă': '🐢', 'Reptilă': '🐍'
    };
    return map[species] || '🐾';
  }

  formatDateTime(dt: string): string {
    return new Date(dt).toLocaleDateString('ro-RO', {
      day: '2-digit', month: 'long', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
  }
}