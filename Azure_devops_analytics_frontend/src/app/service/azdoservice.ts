import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SprintChangeLog, WorkItemCount, WorkItemsResponse } from '../model/models';

@Injectable({
  providedIn: 'root',
})
export class Azdoservice {
  private apiUrl = 'http://localhost:5000/api/AzureDevOps';

  constructor(private http: HttpClient) {}
  
  getWiCount(): Observable<WorkItemCount> {
    return this.http.get<WorkItemCount>(`${this.apiUrl}/wi-count`);
  }

  getSprintChanges(): Observable<SprintChangeLog> {
    return this.http.get<SprintChangeLog>(`${this.apiUrl}/sprint-changes`);
  }

  getWorkItems(): Observable<WorkItemsResponse> {
    return this.http.get<WorkItemsResponse>(`${this.apiUrl}/created-after-start`);
  }
}
