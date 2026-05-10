import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ClinicMapComponent } from './features/clinics/clinic-map/clinic-map.component';
import { MyPetsComponent } from './features/pets/my-pets/my-pets.component';
import { CreateAppointmentComponent } from './features/appointments/create-appointment/create-appointment.component';
import { AuthGuard } from './core/guards/auth.guard';
import { AppointmentsHistoryComponent } from './features/appointments/appointments-history/appointments-history.component';
import { RegisterClinicComponent } from './features/clinics/register-clinic/register-clinic.component';
import { ClinicDashboardComponent } from './features/clinics/clinic-dashboard/clinic-dashboard.component'; // ← adaugă

const routes: Routes = [
  { path: '', redirectTo: '/clinics', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'clinics', component: ClinicMapComponent, canActivate: [AuthGuard] },
  { path: 'my-pets', component: MyPetsComponent, canActivate: [AuthGuard] },
  { path: 'appointments/new', component: CreateAppointmentComponent, canActivate: [AuthGuard] },
  { path: 'my-appointments', component: AppointmentsHistoryComponent, canActivate: [AuthGuard] },
  { path: 'register-clinic', component: RegisterClinicComponent, canActivate: [AuthGuard] },
  { path: 'clinic-dashboard', component: ClinicDashboardComponent, canActivate: [AuthGuard] }, // ← adaugă
  { path: '**', redirectTo: '/clinics' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }