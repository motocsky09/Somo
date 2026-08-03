import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-clinic-dashboard',
  templateUrl: './clinic-dashboard.component.html',
  styleUrls: ['./clinic-dashboard.component.css']
})
export class ClinicDashboardComponent implements OnInit {
  myClinics: any[] = [];
  approvedClinics: any[] = [];
  pendingClinics: any[] = [];
  rejectedClinics: any[] = [];
  selectedClinic: any = null;
  appointments: any[] = [];
  pets: any[] = [];
  vets: any[] = [];
  isLoading = true;
  isLoadingDetails = false;

 
  showAddVetForm = false;
  isSubmittingVet = false;
  vetError = '';
  vetSuccess = '';
  newVet = {
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    specialization: '',
    clinicIds: [] as string[]
  };

  statusLabels: { [key: number]: string } = {
    0: 'În așteptare',
    1: 'Confirmat',
    2: 'Anulat',
    3: 'Finalizat',
    4: 'Nefinalizată'
  };

  statusColors: { [key: number]: string } = {
    0: '#f39c12',
    1: '#2ecc71',
    2: '#e74c3c',
    3: '#3498db',
    4: '#c0392b' 
  };

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private router: Router 
  ) { }
  
  openAppointment(id: string): void {
    this.router.navigate(['/appointment', id]);
  }
  ngOnInit(): void {
    this.http.get<any[]>(`${environment.apiUrl}/Clinics/my-clinics`).subscribe({
      next: (clinics) => {
        this.myClinics = clinics;
        this.approvedClinics = clinics.filter(c => c.status === 'Approved');
        this.pendingClinics = clinics.filter(c => c.status === 'Pending');
        this.rejectedClinics = clinics.filter(c => c.status === 'Rejected');
        this.isLoading = false;

        if (this.approvedClinics.length >= 1) {
          this.selectClinic(this.approvedClinics[0]);
        }
      },
      error: () => { this.isLoading = false; }
    });
  }

  get hasApprovedClinic(): boolean {
    return this.approvedClinics.length > 0;
  }

  get awaitsApproval(): boolean {
    return !this.hasApprovedClinic && this.pendingClinics.length > 0;
  }

  get wasRejected(): boolean {
    return !this.hasApprovedClinic && this.pendingClinics.length === 0 && this.rejectedClinics.length > 0;
  }

  selectClinic(clinic: any): void {
    this.selectedClinic = clinic;
    this.isLoadingDetails = true;
    this.appointments = [];
    this.pets = [];
    this.vets = [];
    this.showAddVetForm = false;

    this.http.get<any[]>(`${environment.apiUrl}/Vets`).subscribe(allVets => {
      this.vets = allVets.filter(v => v.clinicIds.includes(clinic.id));
    });

    this.http.get<any[]>(`${environment.apiUrl}/Appointments/by-clinic/${clinic.id}`).subscribe({
      next: (apps) => {
        this.appointments = apps.sort((a, b) =>
          new Date(b.dateTime).getTime() - new Date(a.dateTime).getTime()
        );

        this.pets = this.appointments
          .map(a => a.pet)
          .filter(pet => !!pet);

        this.isLoadingDetails = false;
      },
      error: () => { this.isLoadingDetails = false; }
    });
  }

  toggleAddVetForm(): void {
    this.showAddVetForm = !this.showAddVetForm;
    this.vetError = '';
    this.vetSuccess = '';
    this.newVet = {
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      specialization: '',
      clinicIds: [this.selectedClinic.id]
    };
  }

  submitVet(): void {
    if (!this.newVet.firstName || !this.newVet.lastName || !this.newVet.specialization) {
      this.vetError = 'Completează cel puțin numele și specializarea.';
      return;
    }

    this.isSubmittingVet = true;
    this.vetError = '';

    this.http.post<any>(`${environment.apiUrl}/Vets`, this.newVet).subscribe({
      next: (vet) => {
        this.vets.push(vet);
        this.vetSuccess = `Dr. ${vet.firstName} ${vet.lastName} a fost adăugat cu succes!`;
        this.isSubmittingVet = false;
        setTimeout(() => {
          this.showAddVetForm = false;
          this.vetSuccess = '';
        }, 2000);
      },
      error: () => {
        this.vetError = 'Eroare la adăugarea medicului. Încearcă din nou.';
        this.isSubmittingVet = false;
      }
    });
  }

  getPetName(app: any): string {
    return app?.pet?.name || 'Animal necunoscut';
  }

  getPetSummary(app: any): string {
    const pet = app?.pet;
    if (!pet) return '';
    return [pet.species, pet.breed].filter(v => !!v).join(' · ');
  }

  getVetName(app: any): string {
    const vet = app?.vet;
    if (vet) return `Dr. ${vet.firstName} ${vet.lastName}`;

    const fallback = this.vets.find(v => v.id === app?.vetId);
    return fallback ? `Dr. ${fallback.firstName} ${fallback.lastName}` : 'Medic necunoscut';
  }

  getOwnerName(app: any): string {
    const owner = app?.owner;
    if (!owner) return 'Proprietar necunoscut';
    const fullName = [owner.firstName, owner.lastName].filter(n => !!n).join(' ');
    return fullName || owner.username;
  }

  getOwnerContact(app: any): string {
    const owner = app?.owner;
    if (!owner) return '';
    return [owner.phone, owner.email].filter(v => !!v).join(' · ');
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

  getAppointmentsByStatus(status: number): any[] {
    return this.appointments.filter(a => a.status === status);
  }

  getUniquePets(): any[] {
    const seen = new Set();
    return this.pets.filter(p => {
      if (seen.has(p.id)) return false;
      seen.add(p.id);
      return true;
    });
  }
  getDisplayStatus(app: any): number {
    if (app.status === 3 || app.status === 2) return app.status;
    const isPast = new Date(app.dateTime) < new Date();
    if (isPast && app.status !== 3) return 4;
    return app.status;
  }
  
  getDisplayStatusLabel(app: any): string {
    return this.statusLabels[this.getDisplayStatus(app)];
  }
  
  getDisplayStatusColor(app: any): string {
    return this.statusColors[this.getDisplayStatus(app)];
  }
  
  isPastAndNotCompleted(app: any): boolean {
    return new Date(app.dateTime) < new Date() && app.status !== 3 && app.status !== 2;
  }
  
  canComplete(app: any): boolean {
    return app.status !== 3 && app.status !== 2;
  }
  
  completeAppointment(app: any): void {
    this.http.patch(`${environment.apiUrl}/Appointments/${app.id}/status`, 3).subscribe({
      next: () => {
        app.status = 3;
      },
      error: () => alert('Eroare la actualizarea programării.')
    });
  }
}