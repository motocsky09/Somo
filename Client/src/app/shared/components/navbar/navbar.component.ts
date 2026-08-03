import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
  isMenuOpen = false;

  constructor(public authService: AuthService, private router: Router) {}

  get homeLink(): string {
    return this.authService.homeRoute;
  }

  get canEditProfile(): boolean {
    return this.authService.isLoggedIn && !this.authService.isClinicAdmin;
  }

  get initials(): string {
    const name = this.authService.displayName.trim();
    if (!name) return '?';
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map(part => part.charAt(0).toUpperCase())
      .join('');
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  closeMenu(): void {
    this.isMenuOpen = false;
  }
}