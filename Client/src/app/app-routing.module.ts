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
import { ClinicDashboardComponent } from './features/clinics/clinic-dashboard/clinic-dashboard.component';
import { AppointmentDetailComponent } from './features/appointments/appointment-detail/appointment-detail.component';
import { HomeComponent } from './features/home/home.component';
import { PetDetailComponent } from './features/pets/pet-detail/pet-detail.component';
import { AdminDashboardComponent } from './features/admin/admin-dashboard/admin-dashboard.component';
import { RoleGuard } from './core/guards/role.guard';
import { HomeGuard } from './core/guards/home.guard';
import { ProfileComponent } from './features/profile/profile.component';


const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'home', component: HomeComponent, canActivate: [HomeGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'clinics', component: ClinicMapComponent, canActivate: [AuthGuard] },
  { path: 'my-pets', component: MyPetsComponent, canActivate: [AuthGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
  { path: 'pets/:id', component: PetDetailComponent, canActivate: [AuthGuard] },
  { path: 'appointments/new', component: CreateAppointmentComponent, canActivate: [AuthGuard] },
  { path: 'my-appointments', component: AppointmentsHistoryComponent, canActivate: [AuthGuard] },
  {
    path: 'register-clinic',
    component: RegisterClinicComponent,
    canActivate: [RoleGuard],
    data: { roles: ['ClinicAdmin'] }
  },
  {
    path: 'clinic-dashboard',
    component: ClinicDashboardComponent,
    canActivate: [RoleGuard],
    data: { roles: ['ClinicAdmin'] }
  },
  {
    path: 'admin',
    component: AdminDashboardComponent,
    canActivate: [RoleGuard],
    data: { roles: ['SomoAdmin'] }
  },
  { path: 'appointment/:id', component: AppointmentDetailComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: '/home' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }