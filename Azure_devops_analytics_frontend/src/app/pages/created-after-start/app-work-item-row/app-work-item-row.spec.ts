import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppWorkItemRow } from './app-work-item-row';

describe('AppWorkItemRow', () => {
  let component: AppWorkItemRow;
  let fixture: ComponentFixture<AppWorkItemRow>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppWorkItemRow]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppWorkItemRow);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
