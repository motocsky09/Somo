import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, RegisterModel } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  errorMessage = '';
  successMessage = '';
  infoMessage = '';
  isLoading = false;
  submittedAsClinic = false;

  scheduleOptions = [
    'Luni-Vineri 08:00-16:00',
    'Luni-Vineri 09:00-17:00',
    'Luni-Vineri 09:00-18:00',
    'Luni-Vineri 10:00-18:00',
    'Luni-Sambata 09:00-17:00',
    'Luni-Sambata 09:00-18:00',
    'Luni-Duminica 09:00-17:00'
  ];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.registerForm = this.fb.group({
      username: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['Owner'],
      clinic: this.fb.group({
        name: [''],
        street: [''],
        streetNumber: [''],
        city: [''],
        county: [''],
        phone: [''],
        email: ['', Validators.email],
        schedule: ['Luni-Vineri 09:00-17:00'],
        vetNames: this.fb.array([this.fb.control('')]),
        prices: this.fb.array([])
      })
    });
  }

  ngOnInit(): void {
    if (this.route.snapshot.queryParams['authRequired']) {
      this.infoMessage = 'Nu sunteți conectat. Creați un cont sau conectați-vă pentru a continua.';
    }

    this.registerForm.get('role')!.valueChanges.subscribe(role => this.applyRoleValidators(role));
  }

  get isClinicAccount(): boolean {
    return this.registerForm.get('role')!.value === 'ClinicAdmin';
  }

  get clinicGroup(): FormGroup {
    return this.registerForm.get('clinic') as FormGroup;
  }

  get vetNames(): FormArray {
    return this.clinicGroup.get('vetNames') as FormArray;
  }

  get prices(): FormArray {
    return this.clinicGroup.get('prices') as FormArray;
  }

  selectRole(role: string): void {
    this.registerForm.get('role')!.setValue(role);
  }

  addVet(): void {
    this.vetNames.push(this.fb.control(''));
  }

  removeVet(index: number): void {
    if (this.vetNames.length > 1) {
      this.vetNames.removeAt(index);
    }
  }

  addPrice(): void {
    this.prices.push(this.fb.group({
      service: [''],
      price: [null]
    }));
  }

  removePrice(index: number): void {
    this.prices.removeAt(index);
  }

  onSubmit(): void {
    this.errorMessage = '';

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    if (this.isClinicAccount && !this.hasNamedVet()) {
      this.errorMessage = 'Adaugă cel puțin un medic veterinar.';
      return;
    }

    this.isLoading = true;
    const wasClinic = this.isClinicAccount;

    this.authService.register(this.buildPayload()).subscribe({
      next: () => {
        this.submittedAsClinic = wasClinic;
        this.successMessage = wasClinic
          ? 'Cererea a fost trimisă. Un administrator Somo o verifică în cel mai scurt timp, iar contul se deblochează după aprobare.'
          : 'Cont creat cu succes! Te redirectăm...';
        setTimeout(() => this.router.navigate(['/login']), wasClinic ? 4000 : 1500);
      },
      error: err => {
        this.errorMessage = err.error?.Message ?? 'Eroare la creare cont. Username-ul poate fi deja folosit.';
        this.isLoading = false;
      }
    });
  }

  private applyRoleValidators(role: string): void {
    const required = ['name', 'street', 'streetNumber', 'city', 'county', 'phone', 'schedule'];

    required.forEach(field => {
      const control = this.clinicGroup.get(field)!;
      control.setValidators(role === 'ClinicAdmin' ? [Validators.required] : []);
      control.updateValueAndValidity({ emitEvent: false });
    });

    const clinicEmail = this.clinicGroup.get('email')!;
    clinicEmail.setValidators(
      role === 'ClinicAdmin'
        ? [Validators.required, Validators.email]
        : [Validators.email]
    );
    clinicEmail.updateValueAndValidity({ emitEvent: false });
  }

  private hasNamedVet(): boolean {
    return this.vetNames.controls.some(c => (c.value ?? '').trim().length > 0);
  }

  private buildPayload(): RegisterModel {
    const { username, email, password, role } = this.registerForm.value;
    const payload: RegisterModel = { username, email, password, role };

    if (role !== 'ClinicAdmin') {
      return payload;
    }

    const clinic = this.clinicGroup.value;

    payload.clinic = {
      name: clinic.name,
      street: clinic.street,
      streetNumber: clinic.streetNumber,
      city: clinic.city,
      county: clinic.county,
      phone: clinic.phone,
      email: clinic.email,
      schedule: clinic.schedule,
      vetNames: (clinic.vetNames as string[])
        .map(v => (v ?? '').trim())
        .filter(v => v.length > 0),
      prices: (clinic.prices as { service: string; price: number }[])
        .filter(p => (p.service ?? '').trim().length > 0 && p.price !== null)
        .map(p => ({ service: p.service.trim(), price: Number(p.price) }))
    };

    return payload;
  }
}
