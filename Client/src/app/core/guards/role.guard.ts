import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (!this.authService.isLoggedIn) {
      this.router.navigate(['/login'], {
        queryParams: { authRequired: '1', returnUrl: state.url }
      });
      return false;
    }

    const allowed = (route.data['roles'] as string[]) ?? [];
    if (allowed.some(role => this.authService.hasRole(role))) {
      return true;
    }

    this.router.navigateByUrl(this.authService.homeRoute);
    return false;
  }
}
