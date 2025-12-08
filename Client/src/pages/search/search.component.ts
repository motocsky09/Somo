import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

interface SearchResult {
  id: string;
  name: string;
}

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnInit {
  query: string = '';
  results: SearchResult[] = [];
  searchPerformed: boolean = false;

  // mock data source
  private DATA: SearchResult[] = [
    { id: '28178854', name: 'INCARICO 28178854 - SMISTAMENTO' },
    { id: '1942761', name: 'INCARICO 1942761 - RECUPERO' },
    { id: '1234567', name: 'DOSAR EXEMPLU 1234567' }
  ];

  constructor(private router: Router, private route: ActivatedRoute) {}

  ngOnInit(): void {
    // read query param when route is /search/results?query=...
    this.route.queryParams.subscribe(params => {
      const q = params['query'] ?? '';
      this.query = q;
      // perform search even when query is empty (shows all results)
      this.performSearch(this.query);
    });
  }

  onSearch(): void {
    // navigate to results URL so the user has a separate page
    // allow empty query to show all results
    this.router.navigate(['/search/result'], { queryParams: { query: this.query } });
  }

  performSearch(q: string) {
    this.searchPerformed = true;
    const lowered = q.toLowerCase();
    this.results = this.DATA.filter(r => r.id.includes(q) || r.name.toLowerCase().includes(lowered));
  }

  openDetails(result: SearchResult) {
    // navigate to details page with id
    this.router.navigate(['/details', result.id]);
  }
}
