import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupportEffortRemaining } from './support-effort-remaining';

describe('SupportHours', () => {
  let component: SupportEffortRemaining;
  let fixture: ComponentFixture<SupportEffortRemaining>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SupportEffortRemaining]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupportEffortRemaining);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
