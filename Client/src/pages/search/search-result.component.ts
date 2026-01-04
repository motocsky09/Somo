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
    { id: 'P-001', name: 'Programare Pisica Mei' },
    { id: 'P-002', name: 'Programare Motan Mango' },
    { id: 'P-003', name: 'Programare Câine Rex' },
    { id: 'P-004', name: 'Programare Papagal Kiki' },
    { id: 'P-005', name: 'Programare Iepure Lulu' }
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
