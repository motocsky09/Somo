import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from '../pages/login/login.component';
import { SearchComponent } from '../pages/search/search.component';
import { SearchResultComponent } from 'src/pages/search/search-result.component';
import { DetailsComponent } from 'src/pages/details/details.component';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'search', component: SearchComponent },
  { path: 'search/result', component: SearchResultComponent },
  { path: 'details/:id', component: DetailsComponent },
  { path: '**', redirectTo: 'login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }