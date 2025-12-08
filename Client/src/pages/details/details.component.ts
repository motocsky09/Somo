import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

// Definim structura datelor din tabel
export interface HistoryElement {
  date: string;
  status: string;
  user: string;
}

// Date false (mock data) ca în poză
const ELEMENT_DATA: HistoryElement[] = [
  {date: '04/12/2025 16:15', status: 'Aperta - Da lavorare', user: 'Illari M.'},
  {date: '04/12/2025 16:15', status: 'Aperta - Ricezione Esito', user: 'Illari M.'},
  {date: '03/12/2025 09:30', status: 'Creazione Incarico', user: 'System'},
];

@Component({
  selector: 'app-details',
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.css']
})
export class DetailsComponent implements OnInit {
  displayedColumns: string[] = ['date', 'status', 'user'];
  historyData = ELEMENT_DATA;
  folderId: string | null = null;
  folderName: string = 'Dosar';

  // richer mock details for the example dosar(s)
  details: any = {
    id: '',
    clientName: '',
    area: '',
    status: '',
    addresses: [] as string[],
    documents: [] as { name: string; date: string }[]
  };

  // static mock database keyed by id
  private DETAILS_DB: Record<string, any> = {
    '28178854': {
      clientName: 'MALACRIDA ALESSANDRO',
      area: 'Recupero Assicurativo',
      status: 'Ricezione Esito Rintraccio',
      addresses: ['Via Colle Eghezzeone 3 - 26000 Lodi (LO) Italia', 'Via Alberto Cuiti 12 - 26010 Montodine (CR) Italia'],
      documents: [
        { name: "Documento Identita'", date: '25/03/2014' },
        { name: 'Polizza', date: '01/01/2020' }
      ]
    },
    '1942761': {
      clientName: 'EXEMPLU CLIENT',
      area: 'Altă Arie',
      status: 'In lucru',
      addresses: ['Str. Exemplu 10, Bucuresti'],
      documents: [{ name: 'Cerere', date: '10/10/2020' }]
    }
  };

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.folderId = id;
        this.folderName = `INCARICO ${id}`;
        const found = this.DETAILS_DB[id];
        if (found) {
          this.details = { id, ...found };
          // set header to match screenshot-like title when possible
          this.folderName = `INCARICO ${id} - ${found.area}`;
        } else {
          // default mock if not found
          this.details = { id, clientName: 'N/A', area: 'N/A', status: 'N/A', addresses: [], documents: [] };
        }
      }
    });
  }
}