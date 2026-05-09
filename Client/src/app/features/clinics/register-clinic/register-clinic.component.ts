import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
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
      address: ['', Validators.required],
      city: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      schedule: ['Luni-Vineri 09:00-17:00', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.clinicForm.invalid) return;

    this.isSubmitting = true;
    this.errorMessage = '';

    this.http.post(
      `${environment.apiUrl}/Clinics/register`,
      this.clinicForm.value
    ).subscribe({
      next: () => {
        this.successMessage = 'Cabinet înregistrat cu succes! Apare acum pe hartă.';
        setTimeout(() => this.router.navigate(['/clinics']), 2000);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Eroare la înregistrare. Încearcă din nou.';
        this.isSubmitting = false;
      }
    });
  }
}