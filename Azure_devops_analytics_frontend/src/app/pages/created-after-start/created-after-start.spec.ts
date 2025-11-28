import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreatedAfterStart } from './created-after-start';

describe('CreatedAfterStart', () => {
  let component: CreatedAfterStart;
  let fixture: ComponentFixture<CreatedAfterStart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreatedAfterStart]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreatedAfterStart);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
