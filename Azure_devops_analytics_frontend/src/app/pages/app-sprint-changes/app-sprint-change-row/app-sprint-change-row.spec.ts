import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppSprintChangeRow } from './app-sprint-change-row';

describe('AppSprintChangeRow', () => {
  let component: AppSprintChangeRow;
  let fixture: ComponentFixture<AppSprintChangeRow>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppSprintChangeRow]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppSprintChangeRow);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
