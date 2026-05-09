import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { GoogleMapsModule } from '@angular/google-maps';
import { JwtInterceptor } from './core/interceptors/jwt.interceptor';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ClinicMapComponent } from './features/clinics/clinic-map/clinic-map.component';
import { MyPetsComponent } from './features/pets/my-pets/my-pets.component';
import { CreateAppointmentComponent } from './features/appointments/create-appointment/create-appointment.component';
import { NavbarComponent } from './shared/components/navbar/navbar.component';
import { AppointmentsHistoryComponent } from './features/appointments/appointments-history/appointments-history.component';
import { RegisterClinicComponent } from './features/clinics/register-clinic/register-clinic.component';


@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    ClinicMapComponent,
    MyPetsComponent,
    CreateAppointmentComponent,
    NavbarComponent,
    AppointmentsHistoryComponent,
    RegisterClinicComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    GoogleMapsModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }