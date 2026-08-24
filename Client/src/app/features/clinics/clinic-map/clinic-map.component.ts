import { Component, OnInit, ViewChild } from '@angular/core';
import { GoogleMap } from '@angular/google-maps';
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
  @ViewChild(GoogleMap) map?: GoogleMap;

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

  /*
   * Pictogramele se cer pe https: pe un site servit securizat, varianta http e
   * blocată ca mixed content și pinul dispare de tot de pe hartă.
   */
  dbMarkerOptions: google.maps.MarkerOptions = {
    icon: {
      url: 'https://maps.google.com/mapfiles/ms/icons/red-dot.png'
    }
  };

  googleMarkerOptions: google.maps.MarkerOptions = {
    icon: {
      url: 'https://maps.google.com/mapfiles/ms/icons/blue-dot.png'
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
          this.frameResults();
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
        this.frameResults();
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

  /** Markerele se recreează doar când chiar se schimbă cabinetul din rând. */
  trackByClinic(_index: number, clinic: Clinic): string {
    return clinic.id;
  }

  trackByGoogleClinic(_index: number, clinic: GoogleClinic): string {
    return clinic.placeId;
  }

  /**
   * Încadrează harta pe toate cabinetele găsite. Fără asta, o căutare într-un
   * oraș unde cabinetele sunt împrăștiate lasă pinurile în afara ecranului și
   * pare că nu există niciunul.
   */
  private frameResults(): void {
    // Harta se randează abia după ce isLoading devine false, deci așteptăm un tick.
    setTimeout(() => {
      const map = this.map?.googleMap;
      if (!map || this.results.length === 0) return;

      const bounds = new google.maps.LatLngBounds();
      bounds.extend(this.center);
      this.results.forEach(result => bounds.extend(result.position));

      map.fitBounds(bounds, 48);

      // Un singur cabinet dă un dreptunghi minuscul; fără plafon s-ar intra în stradă.
      google.maps.event.addListenerOnce(map, 'idle', () => {
        const zoom = map.getZoom();
        if (zoom !== undefined && zoom > 15) {
          map.setZoom(15);
        }
      });
    });
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
