import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

interface SearchResult {
  id: string;
  name: string;
}

@Component({
  selector: 'app-search-result',
  templateUrl: './search-result.component.html',
  styleUrls: ['./search-result.component.css']
})
export class SearchResultComponent implements OnInit {
  query: string = '';
  results: SearchResult[] = [];
  searchPerformed: boolean = false;

  private DATA: SearchResult[] = [
    { id: '28178854', name: 'INCARICO 28178854 - SMISTAMENTO' },
    { id: '1942761', name: 'INCARICO 1942761 - RECUPERO' },
    { id: '1234567', name: 'DOSAR EXEMPLU 1234567' }
  ];

  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.query = params['query'] ?? '';
      this.performSearch(this.query);
    });
  }

  performSearch(q: string) {
    this.searchPerformed = true;
    const lowered = q.toLowerCase();
    this.results = this.DATA.filter(r => r.id.includes(q) || r.name.toLowerCase().includes(lowered));
  }

  openDetails(result: SearchResult) {
    this.router.navigate(['/details', result.id]);
  }
}
