import { Component, OnInit, HostListener, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PetService, Pet } from '../../../core/services/pet.service';
import { AppointmentService, Appointment } from '../../../core/services/appointment.service';
import { ClinicService, Clinic } from '../../../core/services/clinic.service';
import { VetService, Vet } from '../../../core/services/vet.service';
import { AuthService } from '../../../core/services/auth.service';
import { MedicalRecordService, MedicalRecord } from '../../../core/services/medical-record.service';
import { VaccinationService, Vaccination, VaccinationStatus } from '../../../core/services/vaccination.service';

type PetTab = 'appointments' | 'medical' | 'vaccinations';

@Component({
  selector: 'app-pet-detail',
  templateUrl: './pet-detail.component.html',
  styleUrls: ['./pet-detail.component.css']
})
export class PetDetailComponent implements OnInit {
  pet: Pet | null = null;
  appointments: Appointment[] = [];
  clinics: Clinic[] = [];
  vets: Vet[] = [];
  medicalRecords: MedicalRecord[] = [];
  vaccinations: Vaccination[] = [];

  activeTab: PetTab = 'appointments';
  isLoadingChart = false;

  isLoading = true;
  isEditing = false;
  isSaving = false;
  errorMessage = '';

  editName = '';
  editAge = 0;
  editWeight = 0;

  isCropping = false;
  cropSrc = '';
  cropZoom = 1;
  minZoom = 1;
  maxZoom = 3;
  offsetX = 0;
  offsetY = 0;
  dispW = 0;
  dispH = 0;
  readonly viewport = 260;

  private readonly output = 400;
  private cropImg: HTMLImageElement | null = null;
  private baseScale = 1;
  private dragging = false;
  private lastPx = 0;
  private lastPy = 0;

  @ViewChild('photoInput') photoInput!: ElementRef<HTMLInputElement>;

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
    private route: ActivatedRoute,
    private router: Router,
    private petService: PetService,
    private appointmentService: AppointmentService,
    private clinicService: ClinicService,
    private vetService: VetService,
    private authService: AuthService,
    private medicalRecordService: MedicalRecordService,
    private vaccinationService: VaccinationService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/my-pets']);
      return;
    }
    this.clinicService.getAll().subscribe(clinics => this.clinics = clinics);
    this.vetService.getAll().subscribe(vets => this.vets = vets);
    this.loadPet(id);
  }

  loadPet(id: string): void {
    this.petService.getById(id).subscribe({
      next: pet => {
        this.pet = pet;
        this.isLoading = false;
        this.loadAppointments(pet.id);
        this.loadChart(pet.id);
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'Animalul nu a putut fi găsit.';
      }
    });
  }

  loadAppointments(petId: string): void {
    const ownerId = this.authService.currentUser?.id || '';
    this.appointmentService.getMyAppointments(ownerId).subscribe(all => {
      this.appointments = all
        .filter(a => a.petId === petId)
        .sort((a, b) => new Date(b.dateTime).getTime() - new Date(a.dateTime).getTime());
    });
  }

  loadChart(petId: string): void {
    this.isLoadingChart = true;
    let pending = 2;
    const done = () => {
      if (--pending === 0) this.isLoadingChart = false;
    };

    this.medicalRecordService.getByPet(petId).subscribe({
      next: records => { this.medicalRecords = records; done(); },
      error: () => done()
    });

    this.vaccinationService.getByPet(petId).subscribe({
      next: vaccinations => { this.vaccinations = vaccinations; done(); },
      error: () => done()
    });
  }

  setTab(tab: PetTab): void {
    this.activeTab = tab;
  }

  /**
   * Diferența de greutate față de vizita precedentă. Lista vine ordonată
   * descrescător, deci precedenta este următoarea din listă.
   */
  weightChange(index: number): number | null {
    const current = this.medicalRecords[index];
    if (!current || current.weight <= 0) return null;

    const previous = this.medicalRecords
      .slice(index + 1)
      .find(r => r.weight > 0);

    if (!previous) return null;
    return Math.round((current.weight - previous.weight) * 100) / 100;
  }

  vaccinationStatus(vaccination: Vaccination): VaccinationStatus {
    return VaccinationService.statusOf(vaccination);
  }

  vaccinationStatusLabel(vaccination: Vaccination): string {
    switch (this.vaccinationStatus(vaccination)) {
      case 'overdue': return 'Rapel depășit';
      case 'due-soon': return 'Rapel curând';
      default: return 'La zi';
    }
  }

  vaccinationStatusColor(vaccination: Vaccination): string {
    switch (this.vaccinationStatus(vaccination)) {
      case 'overdue': return 'var(--c-e74c3c)';
      case 'due-soon': return 'var(--c-f39c12)';
      default: return 'var(--c-2ecc71)';
    }
  }

  dueDescription(vaccination: Vaccination): string {
    const days = vaccination.daysUntilDue;
    if (days < 0) return `depășit cu ${Math.abs(days)} zile`;
    if (days === 0) return 'scadent astăzi';
    if (days === 1) return 'scadent mâine';
    return `în ${days} zile`;
  }

  /** Cel mai apropiat rapel, folosit pentru avertizarea din capul paginii. */
  get nextVaccination(): Vaccination | null {
    const upcoming = [...this.vaccinations].sort((a, b) => a.daysUntilDue - b.daysUntilDue);
    return upcoming.length > 0 ? upcoming[0] : null;
  }

  get hasVaccinationAlert(): boolean {
    const next = this.nextVaccination;
    return !!next && this.vaccinationStatus(next) !== 'up-to-date';
  }

  startEdit(): void {
    if (!this.pet) return;
    this.editName = this.pet.name;
    this.editAge = this.pet.age;
    this.editWeight = this.pet.weight;
    this.isEditing = true;
    this.errorMessage = '';
  }

  cancelEdit(): void {
    this.isEditing = false;
  }

  saveEdit(): void {
    if (!this.pet) return;
    if (!this.editName.trim()) {
      this.errorMessage = 'Numele este obligatoriu.';
      return;
    }
    const updated: Pet = {
      ...this.pet,
      name: this.editName.trim(),
      age: this.editAge,
      weight: this.editWeight
    };
    this.persist(updated, () => this.isEditing = false);
  }

  onAvatarClick(): void {
    if (this.pet?.photoUrl) {
      this.initCrop(this.pet.photoUrl);
    } else {
      this.photoInput.nativeElement.click();
    }
  }

  triggerChangePhoto(): void {
    this.photoInput.nativeElement.click();
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files && input.files[0];
    if (!file || !this.pet) return;

    if (!file.type.startsWith('image/')) {
      this.errorMessage = 'Fișierul selectat nu este o imagine.';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => this.initCrop(reader.result as string);
    reader.readAsDataURL(file);
    input.value = '';
  }

  setZoom(newZoom: number): void {
    const z = Math.min(this.maxZoom, Math.max(this.minZoom, newZoom));
    const oldEff = this.baseScale * this.cropZoom;
    const newEff = this.baseScale * z;
    const centerX = (this.viewport / 2 - this.offsetX) / oldEff;
    const centerY = (this.viewport / 2 - this.offsetY) / oldEff;
    this.cropZoom = z;
    this.applyDims();
    this.offsetX = this.viewport / 2 - centerX * newEff;
    this.offsetY = this.viewport / 2 - centerY * newEff;
    this.clampOffsets();
  }

  onDragStart(event: PointerEvent): void {
    event.preventDefault();
    this.dragging = true;
    this.lastPx = event.clientX;
    this.lastPy = event.clientY;
  }

  @HostListener('document:pointermove', ['$event'])
  onDragMove(event: PointerEvent): void {
    if (!this.dragging) return;
    this.offsetX += event.clientX - this.lastPx;
    this.offsetY += event.clientY - this.lastPy;
    this.lastPx = event.clientX;
    this.lastPy = event.clientY;
    this.clampOffsets();
  }

  @HostListener('document:pointerup')
  onDragEnd(): void {
    this.dragging = false;
  }

  onWheel(event: WheelEvent): void {
    event.preventDefault();
    this.setZoom(this.cropZoom + (event.deltaY < 0 ? 0.15 : -0.15));
  }

  cancelCrop(): void {
    this.isCropping = false;
    this.cropImg = null;
    this.cropSrc = '';
  }

  confirmCrop(): void {
    if (!this.cropImg || !this.pet) return;
    const eff = this.baseScale * this.cropZoom;
    const sourceSize = this.viewport / eff;

    const canvas = document.createElement('canvas');
    canvas.width = this.output;
    canvas.height = this.output;
    canvas.getContext('2d')?.drawImage(
      this.cropImg,
      -this.offsetX / eff, -this.offsetY / eff, sourceSize, sourceSize,
      0, 0, this.output, this.output
    );

    const updated: Pet = { ...this.pet, photoUrl: canvas.toDataURL('image/jpeg', 0.85) };
    this.persist(updated, () => this.cancelCrop());
  }

  removePhoto(): void {
    if (!this.pet) return;
    this.persist({ ...this.pet, photoUrl: null }, () => this.cancelCrop());
  }

  newAppointment(): void {
    this.router.navigate(['/appointments/new'], { queryParams: { petId: this.pet?.id } });
  }

  getClinicName(clinicId: string): string {
    return this.clinics.find(c => c.id === clinicId)?.name || 'Cabinet necunoscut';
  }

  getVetName(vetId: string): string {
    const vet = this.vets.find(v => v.id === vetId);
    return vet ? `Dr. ${vet.firstName} ${vet.lastName}` : 'Medic necunoscut';
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

  getSpeciesEmoji(species: string): string {
    return PetService.speciesEmoji(species);
  }

  private initCrop(src: string): void {
    const img = new Image();
    img.onload = () => {
      this.cropImg = img;
      this.cropSrc = src;
      this.cropZoom = 1;
      this.baseScale = Math.max(this.viewport / img.width, this.viewport / img.height);
      this.applyDims();
      this.offsetX = (this.viewport - this.dispW) / 2;
      this.offsetY = (this.viewport - this.dispH) / 2;
      this.errorMessage = '';
      this.isCropping = true;
    };
    img.src = src;
  }

  private applyDims(): void {
    const eff = this.baseScale * this.cropZoom;
    this.dispW = (this.cropImg?.width || 0) * eff;
    this.dispH = (this.cropImg?.height || 0) * eff;
  }

  private clampOffsets(): void {
    this.offsetX = Math.min(0, Math.max(this.viewport - this.dispW, this.offsetX));
    this.offsetY = Math.min(0, Math.max(this.viewport - this.dispH, this.offsetY));
  }

  private persist(updated: Pet, onDone?: () => void): void {
    this.isSaving = true;
    this.errorMessage = '';
    this.petService.update(updated.id, updated).subscribe({
      next: pet => {
        this.pet = pet || updated;
        this.isSaving = false;
        onDone?.();
      },
      error: () => {
        this.errorMessage = 'Eroare la salvarea modificărilor.';
        this.isSaving = false;
      }
    });
  }
}
