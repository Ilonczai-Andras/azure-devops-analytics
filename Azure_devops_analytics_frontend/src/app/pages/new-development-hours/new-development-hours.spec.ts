import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewDevelopmentHours } from './new-development-hours';

describe('NewDevelopmentHours', () => {
  let component: NewDevelopmentHours;
  let fixture: ComponentFixture<NewDevelopmentHours>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewDevelopmentHours]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NewDevelopmentHours);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
