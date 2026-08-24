import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { VetService, VetAccount } from '../../../core/services/vet.service';
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
  createdAccount: VetAccount | null = null;
  copyFeedback = '';
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
    0: 'var(--c-f39c12)',
    1: 'var(--c-2ecc71)',
    2: 'var(--c-e74c3c)',
    3: 'var(--c-3498db)',
    4: 'var(--c-c0392b)' 
  };

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private vetService: VetService,
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
    this.createdAccount = null;
    this.copyFeedback = '';
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
    if (!this.newVet.email) {
      this.vetError = 'Adresa de email este necesară pentru contul medicului.';
      return;
    }

    this.isSubmittingVet = true;
    this.vetError = '';

    this.vetService.create(this.newVet).subscribe({
      next: (account) => {
        this.vets.push(account.vet);
        this.createdAccount = account;
        this.vetSuccess = `Dr. ${account.vet.firstName} ${account.vet.lastName} a fost adăugat.`;
        this.isSubmittingVet = false;
        this.showAddVetForm = false;
      },
      error: (err) => {
        this.vetError = err?.error?.error || 'Eroare la adăugarea medicului. Încearcă din nou.';
        this.isSubmittingVet = false;
      }
    });
  }

  /**
   * Medicii adăugați înainte de introducerea conturilor nu au acces în aplicație;
   * cabinetul le poate genera unul păstrând fișa existentă.
   */
  grantAccount(vet: any): void {
    const email = (vet.email || '').trim() ||
      (prompt(`Adresa de email pentru contul lui Dr. ${vet.firstName} ${vet.lastName}:`) || '').trim();

    if (!email) return;

    this.vetError = '';
    this.vetService.createAccount(vet.id, email).subscribe({
      next: (account) => {
        vet.hasAccount = true;
        vet.email = account.vet.email;
        this.createdAccount = account;
        this.vetSuccess = `Contul lui Dr. ${vet.firstName} ${vet.lastName} a fost creat.`;
      },
      error: (err) => {
        this.vetError = err?.error?.error || 'Contul nu a putut fi creat.';
      }
    });
  }

  /** Credențialele se văd o singură dată, până când cabinetul confirmă că le-a notat. */
  dismissCredentials(): void {
    this.createdAccount = null;
    this.vetSuccess = '';
    this.copyFeedback = '';
  }

  copyCredentials(): void {
    if (!this.createdAccount) return;

    const text =
      `Utilizator: ${this.createdAccount.username}\n` +
      `Parolă temporară: ${this.createdAccount.temporaryPassword}`;

    navigator.clipboard?.writeText(text).then(
      () => {
        this.copyFeedback = 'Datele au fost copiate.';
        setTimeout(() => this.copyFeedback = '', 2500);
      },
      () => this.copyFeedback = 'Copierea nu a funcționat. Notează datele manual.'
    );
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