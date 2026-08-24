import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { VetService, VetProfile, VetPatient, AppointmentDetails } from '../../../core/services/vet.service';
import { AppointmentService } from '../../../core/services/appointment.service';
import { MedicalRecordService, MedicalRecord } from '../../../core/services/medical-record.service';
import {
  VaccinationService, Vaccination, VaccineType, VaccinationStatus
} from '../../../core/services/vaccination.service';
import { PetService } from '../../../core/services/pet.service';

const STATUS_CANCELLED = 2;
const STATUS_COMPLETED = 3;

/** Un rând din tabelul de consultații, în lucru. */
interface RecordDraft {
  date: string;
  diagnosis: string;
  treatment: string;
  notes: string;
  weight: number | null;
  temperature: number | null;
}

/** Un rând din tabelul de vaccinări, în lucru. */
interface VaccineDraft {
  vaccineCode: string;
  administeredOn: string;
  nextDueOn: string;
  batchNumber: string;
  notes: string;
}

@Component({
  selector: 'app-patient-chart',
  templateUrl: './patient-chart.component.html',
  styleUrls: ['./patient-chart.component.css']
})
export class PatientChartComponent implements OnInit {
  petId = '';

  profile: VetProfile | null = null;
  patient: VetPatient | null = null;
  records: MedicalRecord[] = [];
  vaccinations: Vaccination[] = [];
  catalog: VaccineType[] = [];

  isLoading = true;
  loadError = '';

  /** Programarea din care s-a intrat pe fișă, dacă s-a intrat din agendă. */
  appointment: AppointmentDetails | null = null;

  activeTab: 'record' | 'vaccine' = 'record';

  newRecord: RecordDraft = this.emptyRecordDraft();
  recordDraft: RecordDraft = this.emptyRecordDraft();
  editingRecordId: string | null = null;
  isSavingRecord = false;
  recordError = '';
  recordSuccess = '';

  newVaccine: VaccineDraft = this.emptyVaccineDraft();
  vaccineDraft: VaccineDraft = this.emptyVaccineDraft();
  editingVaccineId: string | null = null;
  isSavingVaccine = false;
  vaccineError = '';
  vaccineSuccess = '';

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

  vaccineStatusLabels: { [key in VaccinationStatus]: string } = {
    'overdue': 'Rapel depășit',
    'due-soon': 'Rapel curând',
    'up-to-date': 'La zi'
  };

  constructor(
    private route: ActivatedRoute,
    private vetService: VetService,
    private appointmentService: AppointmentService,
    private medicalRecordService: MedicalRecordService,
    private vaccinationService: VaccinationService
  ) {}

  ngOnInit(): void {
    this.vetService.getMyProfile().subscribe({
      next: profile => this.profile = profile
    });

    this.route.paramMap.subscribe(params => {
      this.petId = params.get('petId') || '';
      this.loadPatient();
    });
  }

  private loadPatient(): void {
    if (!this.petId) return;

    this.isLoading = true;
    this.loadError = '';

    this.vetService.getMyPatient(this.petId).subscribe({
      next: patient => {
        this.patient = patient;
        this.appointment = this.findAppointment(patient);
        this.newRecord = this.emptyRecordDraft();
        this.newRecord.weight = patient.pet.weight || null;
        this.isLoading = false;
        this.loadChart(patient.pet.species);
      },
      error: () => {
        this.loadError = 'Nu ai acces la fișa acestui pacient sau animalul nu mai există.';
        this.isLoading = false;
      }
    });
  }

  /**
   * Consultația se leagă de programarea din care s-a deschis fișa; când se intră
   * pe link direct, luăm cea mai recentă programare a medicului cu acest animal.
   */
  private findAppointment(patient: VetPatient): AppointmentDetails | null {
    const requested = this.route.snapshot.queryParamMap.get('appointment');
    if (requested) {
      const match = patient.appointments.find(a => a.id === requested);
      if (match) return match;
    }
    return patient.appointments[0] ?? null;
  }

  private loadChart(species?: string): void {
    this.medicalRecordService.getByPet(this.petId).subscribe({
      next: records => this.records = this.sortRecords(records)
    });

    this.vaccinationService.getByPet(this.petId).subscribe({
      next: vaccinations => this.vaccinations = this.sortVaccinations(vaccinations)
    });

    this.vaccinationService.getCatalog(species).subscribe({
      next: catalog => this.catalog = catalog
    });
  }

  get canWrite(): boolean {
    return !!this.patient?.canWrite;
  }

  get canCompleteAppointment(): boolean {
    return !!this.appointment
      && this.appointment.status !== STATUS_COMPLETED
      && this.appointment.status !== STATUS_CANCELLED;
  }

  completeAppointment(): void {
    if (!this.appointment) return;

    const target = this.appointment;
    const previous = target.status;
    target.status = STATUS_COMPLETED;

    this.appointmentService.updateStatus(target.id, STATUS_COMPLETED).subscribe({
      error: () => {
        target.status = previous;
        this.loadError = 'Starea programării nu a putut fi actualizată.';
        setTimeout(() => this.loadError = '', 4000);
      }
    });
  }

  // ---------------------------------------------------------------- rezumat

  /** Ultima greutate cântărită, sau cea de pe fișa animalului dacă nu s-a cântărit încă. */
  get currentWeight(): number | null {
    const weighed = this.records.find(r => r.weight > 0);
    return weighed?.weight ?? this.patient?.pet.weight ?? null;
  }

  get lastVisit(): MedicalRecord | null {
    return this.records[0] ?? null;
  }

  /**
   * Rapelul cel mai urgent: din fiecare vaccin luăm doar ultima administrare,
   * ca dozele vechi să nu apară veșnic ca depășite.
   */
  get nextBooster(): Vaccination | null {
    const latestPerVaccine = new Map<string, Vaccination>();
    for (const entry of this.vaccinations) {
      const known = latestPerVaccine.get(entry.vaccineCode);
      if (!known || new Date(entry.administeredOn) > new Date(known.administeredOn)) {
        latestPerVaccine.set(entry.vaccineCode, entry);
      }
    }

    return [...latestPerVaccine.values()]
      .sort((a, b) => a.daysUntilDue - b.daysUntilDue)[0] ?? null;
  }

  // ---------------------------------------------------- tabelul de consultații

  /** Doar medicul care a scris rândul îl poate modifica; la fel ca pe server. */
  canEditEntry(vetId: string): boolean {
    return this.canWrite && !!this.profile && this.profile.id === vetId;
  }

  addRecord(): void {
    if (!this.validRecord(this.newRecord)) return;

    this.isSavingRecord = true;

    this.medicalRecordService.create({
      petId: this.petId,
      appointmentId: this.appointment?.id,
      date: this.newRecord.date,
      ...this.recordPayload(this.newRecord)
    }).subscribe({
      next: created => {
        this.records = this.sortRecords([created, ...this.records]);
        this.applyWeightToPatient(created.weight);
        this.newRecord = this.emptyRecordDraft();
        this.isSavingRecord = false;
        this.flashRecordSuccess('Consultația a fost trecută în fișa medicală.');
      },
      error: err => this.failRecord(err, 'Fișa nu a putut fi salvată.')
    });
  }

  startEditRecord(entry: MedicalRecord): void {
    this.editingRecordId = entry.id;
    this.recordError = '';
    this.recordDraft = {
      date: this.toInputDate(new Date(entry.date)),
      diagnosis: entry.diagnosis,
      treatment: entry.treatment,
      notes: entry.notes,
      weight: entry.weight > 0 ? entry.weight : null,
      temperature: entry.temperature > 0 ? entry.temperature : null
    };
  }

  cancelEditRecord(): void {
    this.editingRecordId = null;
    this.recordError = '';
  }

  saveEditRecord(): void {
    if (!this.editingRecordId) return;
    if (!this.validRecord(this.recordDraft)) return;

    const id = this.editingRecordId;
    this.isSavingRecord = true;

    this.medicalRecordService.update(id, {
      date: this.recordDraft.date,
      ...this.recordPayload(this.recordDraft)
    }).subscribe({
      next: updated => {
        this.records = this.sortRecords(this.records.map(r => r.id === id ? updated : r));
        this.applyWeightToPatient(updated.weight);
        this.editingRecordId = null;
        this.isSavingRecord = false;
        this.flashRecordSuccess('Rândul a fost actualizat.');
      },
      error: err => this.failRecord(err, 'Rândul nu a putut fi actualizat.')
    });
  }

  deleteRecord(entry: MedicalRecord): void {
    const label = entry.diagnosis || entry.treatment || 'această consultație';
    if (!window.confirm(`Ștergi definitiv „${label}" din fișă?`)) return;

    this.medicalRecordService.delete(entry.id).subscribe({
      next: () => {
        this.records = this.records.filter(r => r.id !== entry.id);
        this.flashRecordSuccess('Rândul a fost șters din fișă.');
      },
      error: err => this.failRecord(err, 'Rândul nu a putut fi șters.')
    });
  }

  // ------------------------------------------------------ tabelul de vaccinări

  /** Vaccinurile potrivite speciei animalului. */
  get availableVaccines(): VaccineType[] {
    const species = this.patient?.pet.species;
    if (!species) return this.catalog;
    return this.catalog.filter(v => v.species === 'Toate' || v.species === species);
  }

  vaccineTypeOf(code: string): VaccineType | undefined {
    return this.catalog.find(v => v.code === code);
  }

  /** Rapelul propus de schema de vaccinare; medicul îl poate schimba oricând. */
  onVaccineChanged(draft: VaccineDraft): void {
    draft.nextDueOn = this.computeNextDue(draft);
  }

  vaccineStatus(entry: Vaccination): VaccinationStatus {
    return VaccinationService.statusOf(entry);
  }

  addVaccine(): void {
    if (!this.validVaccine(this.newVaccine)) return;

    this.isSavingVaccine = true;

    this.vaccinationService.create({
      petId: this.petId,
      ...this.vaccinePayload(this.newVaccine)
    }).subscribe({
      next: created => {
        this.vaccinations = this.sortVaccinations([created, ...this.vaccinations]);
        this.newVaccine = this.emptyVaccineDraft();
        this.isSavingVaccine = false;
        this.flashVaccineSuccess(
          `${created.vaccineName} a fost trecut în carnet. Proprietarul primește reminder înainte de rapel.`);
      },
      error: err => this.failVaccine(err, 'Vaccinul nu a putut fi salvat.')
    });
  }

  startEditVaccine(entry: Vaccination): void {
    this.editingVaccineId = entry.id;
    this.vaccineError = '';
    this.vaccineDraft = {
      vaccineCode: entry.vaccineCode,
      administeredOn: this.toInputDate(new Date(entry.administeredOn)),
      nextDueOn: this.toInputDate(new Date(entry.nextDueOn)),
      batchNumber: entry.batchNumber,
      notes: entry.notes
    };
  }

  cancelEditVaccine(): void {
    this.editingVaccineId = null;
    this.vaccineError = '';
  }

  saveEditVaccine(): void {
    if (!this.editingVaccineId) return;
    if (!this.validVaccine(this.vaccineDraft)) return;

    const id = this.editingVaccineId;
    this.isSavingVaccine = true;

    this.vaccinationService.update(id, this.vaccinePayload(this.vaccineDraft)).subscribe({
      next: updated => {
        this.vaccinations = this.sortVaccinations(
          this.vaccinations.map(v => v.id === id ? updated : v)
        );
        this.editingVaccineId = null;
        this.isSavingVaccine = false;
        this.flashVaccineSuccess('Vaccinul a fost actualizat.');
      },
      error: err => this.failVaccine(err, 'Vaccinul nu a putut fi actualizat.')
    });
  }

  deleteVaccine(entry: Vaccination): void {
    if (!window.confirm(`Ștergi „${entry.vaccineName}" din carnetul de vaccinare?`)) return;

    this.vaccinationService.delete(entry.id).subscribe({
      next: () => {
        this.vaccinations = this.vaccinations.filter(v => v.id !== entry.id);
        this.flashVaccineSuccess('Vaccinul a fost șters din carnet.');
      },
      error: err => this.failVaccine(err, 'Vaccinul nu a putut fi șters.')
    });
  }

  // ------------------------------------------------------------------ ajutoare

  getSpeciesEmoji(species?: string): string {
    return PetService.speciesEmoji(species || '');
  }

  formatDate(dateTime: string): string {
    return new Date(dateTime).toLocaleDateString('ro-RO', {
      day: '2-digit', month: 'short', year: 'numeric'
    });
  }

  formatTime(dateTime: string): string {
    return new Date(dateTime).toLocaleTimeString('ro-RO', {
      hour: '2-digit', minute: '2-digit'
    });
  }

  private validRecord(draft: RecordDraft): boolean {
    if (!draft.diagnosis.trim() && !draft.treatment.trim()) {
      this.recordError = 'Completează cel puțin diagnosticul sau tratamentul.';
      return false;
    }
    this.recordError = '';
    return true;
  }

  private recordPayload(draft: RecordDraft) {
    return {
      diagnosis: draft.diagnosis.trim(),
      treatment: draft.treatment.trim(),
      notes: draft.notes.trim(),
      weight: Number(draft.weight) || 0,
      temperature: Number(draft.temperature) || 0
    };
  }

  private validVaccine(draft: VaccineDraft): boolean {
    if (!draft.vaccineCode) {
      this.vaccineError = 'Alege vaccinul din schema de vaccinare.';
      return false;
    }
    if (!draft.administeredOn) {
      this.vaccineError = 'Completează data administrării.';
      return false;
    }
    this.vaccineError = '';
    return true;
  }

  private vaccinePayload(draft: VaccineDraft) {
    return {
      vaccineCode: draft.vaccineCode,
      administeredOn: draft.administeredOn,
      nextDueOn: draft.nextDueOn || null,
      batchNumber: draft.batchNumber.trim(),
      notes: draft.notes.trim()
    };
  }

  private flashRecordSuccess(message: string): void {
    this.recordError = '';
    this.recordSuccess = message;
    setTimeout(() => this.recordSuccess = '', 3000);
  }

  private failRecord(err: any, fallback: string): void {
    this.recordError = err?.error?.error || fallback;
    this.isSavingRecord = false;
  }

  private flashVaccineSuccess(message: string): void {
    this.vaccineError = '';
    this.vaccineSuccess = message;
    setTimeout(() => this.vaccineSuccess = '', 4000);
  }

  private failVaccine(err: any, fallback: string): void {
    this.vaccineError = err?.error?.error || fallback;
    this.isSavingVaccine = false;
  }

  private sortRecords(records: MedicalRecord[]): MedicalRecord[] {
    return [...records].sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
  }

  private sortVaccinations(vaccinations: Vaccination[]): Vaccination[] {
    return [...vaccinations].sort(
      (a, b) => new Date(b.administeredOn).getTime() - new Date(a.administeredOn).getTime());
  }

  private emptyRecordDraft(): RecordDraft {
    return {
      date: this.today(),
      diagnosis: '',
      treatment: '',
      notes: '',
      weight: null,
      temperature: null
    };
  }

  private emptyVaccineDraft(): VaccineDraft {
    return {
      vaccineCode: '',
      administeredOn: this.today(),
      nextDueOn: '',
      batchNumber: '',
      notes: ''
    };
  }

  private computeNextDue(draft: VaccineDraft): string {
    const type = this.vaccineTypeOf(draft.vaccineCode);
    if (!type || !draft.administeredOn) return '';

    const administered = new Date(draft.administeredOn);
    if (isNaN(administered.getTime())) return '';

    const due = new Date(administered);
    due.setMonth(due.getMonth() + type.intervalMonths);
    return this.toInputDate(due);
  }

  /** Greutatea proaspăt măsurată se vede imediat în antetul paginii. */
  private applyWeightToPatient(weight: number): void {
    if (weight > 0 && this.patient) {
      this.patient.pet.weight = weight;
    }
  }

  private today(): string {
    return this.toInputDate(new Date());
  }

  private toInputDate(date: Date): string {
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${date.getFullYear()}-${month}-${day}`;
  }
}
