import { Component, OnInit } from '@angular/core';
import { VetService, VetProfile, AppointmentDetails } from '../../../core/services/vet.service';
import { AppointmentService } from '../../../core/services/appointment.service';
import { PetService } from '../../../core/services/pet.service';

type AgendaFilter = 'today' | 'upcoming' | 'past';

const STATUS_PENDING = 0;
const STATUS_CONFIRMED = 1;
const STATUS_CANCELLED = 2;
const STATUS_COMPLETED = 3;

@Component({
  selector: 'app-vet-dashboard',
  templateUrl: './vet-dashboard.component.html',
  styleUrls: ['./vet-dashboard.component.css']
})
export class VetDashboardComponent implements OnInit {
  profile: VetProfile | null = null;
  appointments: AppointmentDetails[] = [];

  isLoading = true;
  loadError = '';
  activeFilter: AgendaFilter = 'today';

  statusLabels: { [key: number]: string } = {
    0: 'În așteptare',
    1: 'Confirmat',
    2: 'Anulat',
    3: 'Finalizat'
  };

  statusColors: { [key: number]: string } = {
    0: 'var(--c-f39c12)',
    1: 'var(--c-2ecc71)',
    2: 'var(--c-e74c3c)',
    3: 'var(--c-3498db)'
  };

  constructor(
    private vetService: VetService,
    private appointmentService: AppointmentService
  ) {}

  ngOnInit(): void {
    this.vetService.getMyProfile().subscribe({
      next: profile => this.profile = profile,
      error: () => this.loadError =
        'Contul tău nu este legat de o fișă de medic. Contactează cabinetul care ți-a creat contul.'
    });

    this.vetService.getMyAppointments().subscribe({
      next: appointments => {
        this.appointments = appointments;
        this.isLoading = false;
        if (this.todayAppointments.length === 0 && this.upcomingAppointments.length > 0) {
          this.activeFilter = 'upcoming';
        }
      },
      error: () => this.isLoading = false
    });
  }

  get todayAppointments(): AppointmentDetails[] {
    const today = new Date().toDateString();
    return this.appointments
      .filter(a => new Date(a.dateTime).toDateString() === today)
      .sort((a, b) => this.time(a) - this.time(b));
  }

  get upcomingAppointments(): AppointmentDetails[] {
    const startOfTomorrow = new Date();
    startOfTomorrow.setHours(24, 0, 0, 0);
    return this.appointments
      .filter(a => new Date(a.dateTime) >= startOfTomorrow)
      .sort((a, b) => this.time(a) - this.time(b));
  }

  get pastAppointments(): AppointmentDetails[] {
    const startOfToday = new Date();
    startOfToday.setHours(0, 0, 0, 0);
    return this.appointments
      .filter(a => new Date(a.dateTime) < startOfToday)
      .sort((a, b) => this.time(b) - this.time(a));
  }

  get visibleAppointments(): AppointmentDetails[] {
    switch (this.activeFilter) {
      case 'upcoming': return this.upcomingAppointments;
      case 'past': return this.pastAppointments;
      default: return this.todayAppointments;
    }
  }

  /** Programări de azi care încă așteaptă confirmarea sau consultația. */
  get openTodayCount(): number {
    return this.todayAppointments
      .filter(a => a.status !== STATUS_COMPLETED && a.status !== STATUS_CANCELLED)
      .length;
  }

  setFilter(filter: AgendaFilter): void {
    this.activeFilter = filter;
  }

  canConfirm(appointment: AppointmentDetails): boolean {
    return appointment.status === STATUS_PENDING;
  }

  canComplete(appointment: AppointmentDetails): boolean {
    return appointment.status !== STATUS_COMPLETED && appointment.status !== STATUS_CANCELLED;
  }

  confirm(appointment: AppointmentDetails): void {
    this.changeStatus(appointment, STATUS_CONFIRMED);
  }

  cancel(appointment: AppointmentDetails): void {
    this.changeStatus(appointment, STATUS_CANCELLED);
  }

  getSpeciesEmoji(species?: string): string {
    return PetService.speciesEmoji(species || '');
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

  private changeStatus(appointment: AppointmentDetails, status: number): void {
    const previous = appointment.status;
    appointment.status = status;

    this.appointmentService.updateStatus(appointment.id, status).subscribe({
      error: () => {
        appointment.status = previous;
        this.loadError = 'Starea programării nu a putut fi actualizată.';
        setTimeout(() => this.loadError = '', 4000);
      }
    });
  }

  private time(appointment: AppointmentDetails): number {
    return new Date(appointment.dateTime).getTime();
  }
}
