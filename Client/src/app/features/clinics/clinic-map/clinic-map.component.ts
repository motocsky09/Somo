import { Component, OnInit } from '@angular/core';
import { ClinicService, Clinic, GoogleClinic } from '../../../core/services/clinic.service';

@Component({
  selector: 'app-clinic-map',
  templateUrl: './clinic-map.component.html',
  styleUrls: ['./clinic-map.component.css']
})
export class ClinicMapComponent implements OnInit {
  center: google.maps.LatLngLiteral = { lat: 47.0722, lng: 21.9215 };
  zoom = 13;
  dbClinics: Clinic[] = [];
  googleClinics: GoogleClinic[] = [];
  selectedClinic: Clinic | null = null;
  selectedGoogleClinic: GoogleClinic | null = null;
  isLoading = true;
  radiusKm = 10;

  mapOptions: google.maps.MapOptions = {
    mapTypeId: 'roadmap',
    zoomControl: true,
    scrollwheel: true,
    disableDoubleClickZoom: false,
    maxZoom: 20,
    minZoom: 5
  };

  dbMarkerOptions: google.maps.MarkerOptions = {
    icon: {
      url: 'http://maps.google.com/mapfiles/ms/icons/red-dot.png'
    }
  };

  googleMarkerOptions: google.maps.MarkerOptions = {
    icon: {
      url: 'http://maps.google.com/mapfiles/ms/icons/blue-dot.png'
    }
  };

  constructor(private clinicService: ClinicService) {}

  ngOnInit(): void {
    this.getUserLocation();
  }

  getUserLocation(): void {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        position => {
          this.center = {
            lat: position.coords.latitude,
            lng: position.coords.longitude
          };
          this.loadNearbyClinics();
        },
        () => this.loadNearbyClinics()
      );
    } else {
      this.loadNearbyClinics();
    }
  }

  loadNearbyClinics(): void {
    this.isLoading = true;
    this.clinicService.getNearby(this.center.lat, this.center.lng, this.radiusKm)
      .subscribe({
        next: response => {
          this.dbClinics = response.databaseClinics;
          this.googleClinics = response.googleClinics.filter(g => !g.isInDatabase);
          this.isLoading = false;
        },
        error: () => this.isLoading = false
      });
  }

  onDbMarkerClick(clinic: Clinic): void {
    this.selectedClinic = clinic;
    this.selectedGoogleClinic = null;
  }

  onGoogleMarkerClick(clinic: GoogleClinic): void {
    this.selectedGoogleClinic = clinic;
    this.selectedClinic = null;
  }

  closePanel(): void {
    this.selectedClinic = null;
    this.selectedGoogleClinic = null;
  }

  getDbMarkerPosition(clinic: Clinic): google.maps.LatLngLiteral {
    return { lat: clinic.latitude, lng: clinic.longitude };
  }

  getGoogleMarkerPosition(clinic: GoogleClinic): google.maps.LatLngLiteral {
    return { lat: clinic.latitude, lng: clinic.longitude };
  }

  get totalClinics(): number {
    return this.dbClinics.length + this.googleClinics.length;
  }
}