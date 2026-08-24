import { Component, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnDestroy {
  isMenuOpen = false;

  constructor(public authService: AuthService, public themeService: ThemeService, private router: Router) {}

  ngOnDestroy(): void {
    this.setMenu(false);
  }

  get homeLink(): string {
    return this.authService.homeRoute;
  }

  get canEditProfile(): boolean {
    return this.authService.isOwner || this.authService.isVet;
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
    this.closeMenu();
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  toggleMenu(): void {
    this.setMenu(!this.isMenuOpen);
  }

  closeMenu(): void {
    this.setMenu(false);
  }

  private setMenu(open: boolean): void {
    this.isMenuOpen = open;
    document.body.style.overflow = open ? 'hidden' : '';
  }
}