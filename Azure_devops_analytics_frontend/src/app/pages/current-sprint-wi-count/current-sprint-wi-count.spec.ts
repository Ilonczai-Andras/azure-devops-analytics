import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CurrentSprintWiCount } from './current-sprint-wi-count';

describe('CurrentSprintWiCount', () => {
  let component: CurrentSprintWiCount;
  let fixture: ComponentFixture<CurrentSprintWiCount>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CurrentSprintWiCount]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CurrentSprintWiCount);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
