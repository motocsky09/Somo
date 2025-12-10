import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css'],
  standalone: true,
  imports: [FormsModule]
})
export class SearchComponent {
  query: string = '';

  constructor(private router: Router) {}

  onSearch(): void {
    // navigate to results URL so the user has a separate page
    // allow empty query to show all results
    this.router.navigate(['/search/result'], { queryParams: { query: this.query } });
  }
}

