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
  folderName = 'INCARICO 28178854';
  details = {
    clientName: 'AXA FRANCE IARD',
    area: 'Recupero Assicurativo',
    status: 'Ricezione Esito Rintraccio'
  };
  folderId = '28178854';

  historyData = [
    {date: '04/12/2025 16:15', status: 'Aperta - Da lavorare', user: 'Illari M.'},
    {date: '04/12/2025 16:15', status: 'Aperta - Ricezione Esito', user: 'Illari M.'},
    {date: '03/12/2025 09:30', status: 'Creazione Incarico', user: 'System'},
  ];
}