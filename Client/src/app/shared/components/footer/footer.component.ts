import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.css']
})
export class FooterComponent {
  currentYear = new Date().getFullYear();

  constructor(public authService: AuthService) {}

  get workspaceLabel(): string {
    return this.authService.isSomoAdmin ? 'Administrarea platformei' : 'Platforma cabinetelor veterinare';
  }
}
