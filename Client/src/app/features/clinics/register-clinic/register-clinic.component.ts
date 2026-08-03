import { Component } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-register-clinic',
  templateUrl: './register-clinic.component.html',
  styleUrls: ['./register-clinic.component.css']
})
export class RegisterClinicComponent {
  clinicForm: FormGroup;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  scheduleOptions = [
    'Luni-Vineri 08:00-16:00',
    'Luni-Vineri 09:00-17:00',
    'Luni-Vineri 09:00-18:00',
    'Luni-Vineri 10:00-18:00',
    'Luni-Sambata 09:00-17:00',
    'Luni-Sambata 09:00-18:00',
    'Luni-Duminica 09:00-17:00',
    'Program personalizat'
  ];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router
  ) {
    this.clinicForm = this.fb.group({
      name: ['', Validators.required],
      street: ['', Validators.required],
      streetNumber: ['', Validators.required],
      city: ['', Validators.required],
      county: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      schedule: ['Luni-Vineri 09:00-17:00', Validators.required],
      vetNames: this.fb.array([this.fb.control('')]),
      prices: this.fb.array([])
    });
  }

  get vetNames(): FormArray {
    return this.clinicForm.get('vetNames') as FormArray;
  }

  get prices(): FormArray {
    return this.clinicForm.get('prices') as FormArray;
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

    if (this.clinicForm.invalid) {
      this.clinicForm.markAllAsTouched();
      return;
    }

    const vetNames = (this.vetNames.value as string[])
      .map(v => (v ?? '').trim())
      .filter(v => v.length > 0);

    if (!vetNames.length) {
      this.errorMessage = 'Adaugă cel puțin un medic veterinar.';
      return;
    }

    this.isSubmitting = true;

    const payload = {
      ...this.clinicForm.value,
      vetNames,
      prices: (this.prices.value as { service: string; price: number }[])
        .filter(p => (p.service ?? '').trim().length > 0 && p.price !== null)
        .map(p => ({ service: p.service.trim(), price: Number(p.price) }))
    };

    this.http.post(`${environment.apiUrl}/Clinics/register`, payload).subscribe({
      next: () => {
        this.successMessage = 'Cererea a fost trimisă. Cabinetul apare pe hartă după aprobarea unui administrator Somo.';
        setTimeout(() => this.router.navigate(['/clinic-dashboard']), 3000);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Eroare la înregistrare. Încearcă din nou.';
        this.isSubmitting = false;
      }
    });
  }
}
