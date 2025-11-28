import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SprintChangeLog, WorkItemCount } from '../model/models';

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
}
