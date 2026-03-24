import { Component, OnInit } from '@angular/core';
import { ClinicService, Clinic } from '../../../core/services/clinic.service';

@Component({
  selector: 'app-clinic-map',
  templateUrl: './clinic-map.component.html',
  styleUrls: ['./clinic-map.component.css']
})
export class ClinicMapComponent implements OnInit {
  center: google.maps.LatLngLiteral = { lat: 47.0722, lng: 21.9215 };
  zoom = 13;
  clinics: Clinic[] = [];
  selectedClinic: Clinic | null = null;
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
        () => {
          
          this.loadNearbyClinics();
        }
      );
    } else {
      this.loadNearbyClinics();
    }
  }

  loadNearbyClinics(): void {
    this.isLoading = true;
    this.clinicService.getNearby(this.center.lat, this.center.lng, this.radiusKm)
      .subscribe({
        next: clinics => {
          this.clinics = clinics;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
        }
      });
  }

  onMarkerClick(clinic: Clinic): void {
    this.selectedClinic = clinic;
  }

  closePanel(): void {
    this.selectedClinic = null;
  }

  getMarkerPosition(clinic: Clinic): google.maps.LatLngLiteral {
    return { lat: clinic.latitude, lng: clinic.longitude };
  }
}