import { Component, OnInit } from '@angular/core';
import { ClinicService, Clinic, GoogleClinic } from '../../../core/services/clinic.service';

export interface ClinicResult {
  name: string;
  address: string;
  position: google.maps.LatLngLiteral;
  inApp: boolean;
  clinic?: Clinic;
  googleClinic?: GoogleClinic;
}

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
  results: ClinicResult[] = [];

  cityQuery = '';
  searchedCity = '';
  isSearching = false;
  searchError = '';
  cachedAt: Date | null = null;
  expiresAt: Date | null = null;
  fromCache = false;

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
          this.results = this.buildResults();
          this.isLoading = false;
        },
        error: () => this.isLoading = false
      });
  }

  onRadiusChange(): void {
    if (this.searchedCity) {
      this.searchCity(this.searchedCity);
      return;
    }
    this.loadNearbyClinics();
  }

  searchCity(city: string = this.cityQuery, refresh: boolean = false): void {
    const query = city.trim();
    if (!query) {
      this.searchError = 'Introdu numele orașului.';
      return;
    }

    this.isSearching = true;
    this.searchError = '';
    this.closePanel();

    this.clinicService.searchByCity(query, this.radiusKm, refresh).subscribe({
      next: response => {
        this.center = { lat: response.latitude, lng: response.longitude };
        this.zoom = 13;
        this.dbClinics = response.databaseClinics;
        this.googleClinics = response.googleClinics.filter(g => !g.isInDatabase);
        this.results = this.buildResults();
        this.searchedCity = response.city;
        this.cityQuery = response.city;
        this.fromCache = response.fromCache;
        this.cachedAt = new Date(response.cachedAtUtc);
        this.expiresAt = new Date(response.expiresAtUtc);
        this.isSearching = false;
        this.isLoading = false;
      },
      error: err => {
        this.searchError = err.error?.message ?? 'Căutarea nu a reușit. Încearcă din nou.';
        this.isSearching = false;
      }
    });
  }

  refreshCity(): void {
    if (this.searchedCity) {
      this.searchCity(this.searchedCity, true);
    }
  }

  resetSearch(): void {
    this.cityQuery = '';
    this.searchedCity = '';
    this.searchError = '';
    this.cachedAt = null;
    this.expiresAt = null;
    this.closePanel();
    this.getUserLocation();
  }

  selectResult(result: ClinicResult): void {
    this.center = result.position;
    this.zoom = 16;

    if (result.clinic) {
      this.onDbMarkerClick(result.clinic);
      return;
    }
    if (result.googleClinic) {
      this.onGoogleMarkerClick(result.googleClinic);
    }
  }

  isSelected(result: ClinicResult): boolean {
    if (result.clinic) {
      return this.selectedClinic?.id === result.clinic.id;
    }
    return this.selectedGoogleClinic?.placeId === result.googleClinic?.placeId;
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

  private buildResults(): ClinicResult[] {
    const fromDb = this.dbClinics.map(c => ({
      name: c.name,
      address: `${c.address}, ${c.city}`,
      position: this.getDbMarkerPosition(c),
      inApp: true,
      clinic: c
    }));

    const fromGoogle = this.googleClinics.map(g => ({
      name: g.name,
      address: g.address,
      position: this.getGoogleMarkerPosition(g),
      inApp: false,
      googleClinic: g
    }));

    return [...fromDb, ...fromGoogle];
  }

  get totalClinics(): number {
    return this.dbClinics.length + this.googleClinics.length;
  }
}
