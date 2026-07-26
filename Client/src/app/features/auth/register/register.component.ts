import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

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
      role: ['Owner']
    });
  }

  ngOnInit(): void {
    if (this.route.snapshot.queryParams['authRequired']) {
      this.infoMessage = 'Nu sunteți conectat. Creați un cont sau conectați-vă pentru a continua.';
    }
  }

  onSubmit(): void {
    if (this.registerForm.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.successMessage = 'Cont creat cu succes! Te redirectăm...';
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: () => {
        this.errorMessage = 'Eroare la creare cont. Username-ul poate fi deja folosit.';
        this.isLoading = false;
      }
    });
  }
}