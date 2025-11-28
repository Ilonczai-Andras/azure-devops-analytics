import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SprintCapacity } from './sprint-capacity';

describe('SprintCapacity', () => {
  let component: SprintCapacity;
  let fixture: ComponentFixture<SprintCapacity>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SprintCapacity]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SprintCapacity);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
