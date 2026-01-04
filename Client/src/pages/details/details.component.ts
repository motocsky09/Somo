import { Component } from '@angular/core';

@Component({
  selector: 'app-details',
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.css']
})
export class DetailsComponent {
  // Variabila care controlează ce tab e deschis
  activeTab: string = 'workflow'; 

  // Datele simulate (le păstrăm pe cele vechi)
  // Appointment / patient details (veterinary-themed)
  folderName = 'Programare Pisica Mei';
  details = {
    ownerName: 'Mei Popescu',
    species: 'Pisică — Persan',
    veterinarian: 'Dr. Ana Ionescu',
    contact: '+40 712 345 678',
    scheduledAt: '12/01/2026 10:30',
    status: 'Programată'
  };
  folderId = 'P-001';

  historyData = [
    {date: '05/01/2026 09:12', status: 'Programare creată', user: 'Mei Popescu'},
    {date: '06/01/2026 08:45', status: 'Confirmare telefonică', user: 'Dr. Ana Ionescu'},
    {date: '10/01/2026 11:00', status: 'Notă: vaccin efectuat', user: 'Dr. Ana Ionescu'},
  ];
}