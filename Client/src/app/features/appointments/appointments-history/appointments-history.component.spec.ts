import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppointmentsHistoryComponent } from './appointments-history.component';

describe('AppointmentsHistoryComponent', () => {
  let component: AppointmentsHistoryComponent;
  let fixture: ComponentFixture<AppointmentsHistoryComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [AppointmentsHistoryComponent]
    });
    fixture = TestBed.createComponent(AppointmentsHistoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
