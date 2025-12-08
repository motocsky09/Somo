import { Component } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  showNavbar: boolean = false;

  constructor(private router: Router) {
    // Ascultăm schimbările de rută (pagină)
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        // Dacă linkul conține 'login', ASCUNDEM navbar-ul. Altfel îl arătăm.
        this.showNavbar = !event.url.includes('/login');
      }
    });
  }
}