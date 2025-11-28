import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppSprintChanges } from './app-sprint-changes';

describe('AppSprintChanges', () => {
  let component: AppSprintChanges;
  let fixture: ComponentFixture<AppSprintChanges>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppSprintChanges]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppSprintChanges);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
