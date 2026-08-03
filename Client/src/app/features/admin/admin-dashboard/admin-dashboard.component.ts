import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import {
  AdminService,
  AdminClinic,
  AdminOverview,
  AdminOwner,
  ClinicStatus
} from '../../../core/services/admin.service';

type Tab = 'requests' | 'clinics' | 'owners';

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {
  activeTab: Tab = 'requests';
  overview: AdminOverview | null = null;
  owners: AdminOwner[] = [];
  clinics: AdminClinic[] = [];
  isLoading = true;
  errorMessage = '';
  busyId = '';

  rejectingId = '';
  rejectionReason = '';

  statusLabels: Record<ClinicStatus, string> = {
    Pending: 'În așteptare',
    Approved: 'Aprobat',
    Rejected: 'Respins'
  };

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      overview: this.adminService.getOverview(),
      owners: this.adminService.getOwners(),
      clinics: this.adminService.getClinics()
    }).subscribe({
      next: data => {
        this.overview = data.overview;
        this.owners = data.owners;
        this.clinics = data.clinics;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Datele nu au putut fi încărcate.';
        this.isLoading = false;
      }
    });
  }

  get pendingClinics(): AdminClinic[] {
    return this.clinics.filter(c => c.status === 'Pending');
  }

  get reviewedClinics(): AdminClinic[] {
    return this.clinics.filter(c => c.status !== 'Pending');
  }

  selectTab(tab: Tab): void {
    this.activeTab = tab;
    this.cancelRejection();
  }

  approve(clinic: AdminClinic): void {
    this.busyId = clinic.id;
    this.adminService.approveClinic(clinic.id).subscribe({
      next: updated => {
        this.replaceClinic(updated);
        this.busyId = '';
        this.refreshOverview();
      },
      error: () => {
        this.errorMessage = `Cabinetul "${clinic.name}" nu a putut fi aprobat.`;
        this.busyId = '';
      }
    });
  }

  startRejection(clinic: AdminClinic): void {
    this.rejectingId = clinic.id;
    this.rejectionReason = '';
  }

  cancelRejection(): void {
    this.rejectingId = '';
    this.rejectionReason = '';
  }

  confirmRejection(clinic: AdminClinic): void {
    this.busyId = clinic.id;
    this.adminService.rejectClinic(clinic.id, this.rejectionReason).subscribe({
      next: updated => {
        this.replaceClinic(updated);
        this.cancelRejection();
        this.busyId = '';
        this.refreshOverview();
      },
      error: () => {
        this.errorMessage = `Cabinetul "${clinic.name}" nu a putut fi respins.`;
        this.busyId = '';
      }
    });
  }

  deleteClinic(clinic: AdminClinic): void {
    if (!confirm(`Ștergi definitiv cabinetul "${clinic.name}"? Acțiunea nu poate fi anulată.`)) {
      return;
    }

    this.busyId = clinic.id;
    this.adminService.deleteClinic(clinic.id).subscribe({
      next: () => {
        this.clinics = this.clinics.filter(c => c.id !== clinic.id);
        this.busyId = '';
        this.refreshOverview();
      },
      error: () => {
        this.errorMessage = `Cabinetul "${clinic.name}" nu a putut fi șters.`;
        this.busyId = '';
      }
    });
  }

  deleteOwner(owner: AdminOwner): void {
    const petNote = owner.pets.length
      ? ` Se șterg și cele ${owner.pets.length} animale asociate.`
      : '';

    if (!confirm(`Ștergi definitiv contul "${owner.username}"?${petNote}`)) {
      return;
    }

    this.busyId = owner.id;
    this.adminService.deleteUser(owner.id).subscribe({
      next: () => {
        this.owners = this.owners.filter(o => o.id !== owner.id);
        this.busyId = '';
        this.refreshOverview();
      },
      error: () => {
        this.errorMessage = `Contul "${owner.username}" nu a putut fi șters.`;
        this.busyId = '';
      }
    });
  }

  fullAddress(clinic: AdminClinic): string {
    return [clinic.address, clinic.city, clinic.county]
      .filter(part => part && part.length > 0)
      .join(', ');
  }

  formatDate(value: string | null): string {
    if (!value) return '—';
    return new Date(value).toLocaleDateString('ro-RO', {
      day: '2-digit', month: 'long', year: 'numeric', hour: '2-digit', minute: '2-digit'
    });
  }

  private replaceClinic(updated: AdminClinic): void {
    this.clinics = this.clinics.map(c => (c.id === updated.id ? updated : c));
  }

  private refreshOverview(): void {
    this.adminService.getOverview().subscribe({
      next: overview => this.overview = overview,
      error: () => {}
    });
  }
}
