import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { LoginComponent } from '../pages/login/login.component';
import { SearchComponent } from '../pages/search/search.component';
import { SearchResultComponent } from 'src/pages/search/search-result.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DetailsComponent } from '../pages/details/details.component';


@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    DetailsComponent,
    SearchResultComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    SearchComponent,
    FormsModule,
    ReactiveFormsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
