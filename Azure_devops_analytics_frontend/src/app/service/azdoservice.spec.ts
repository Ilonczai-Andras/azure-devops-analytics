import { TestBed } from '@angular/core/testing';

import { Azdoservice } from './azdoservice';

describe('Azdoservice', () => {
  let service: Azdoservice;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Azdoservice);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
