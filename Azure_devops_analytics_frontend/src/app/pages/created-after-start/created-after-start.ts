import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { AppWorkItemRow } from './app-work-item-row/app-work-item-row';
import { WorkItem } from '../../model/models';
import { Azdoservice } from '../../service/azdoservice';

@Component({
  selector: 'app-created-after-start',
  imports: [AppWorkItemRow],
  templateUrl: './created-after-start.html',
  styleUrl: './created-after-start.css',
})
export class CreatedAfterStart implements OnInit {
  isLoading: boolean = true;
  workItemCount: number = 0;
  workItems: WorkItem[] = [];

  constructor(private azdoService: Azdoservice,  private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadData();
  }

  private loadData() {
    this.azdoService.getWorkItems().subscribe({
      next: (response) => {
        this.workItems = response.workItems || [];
        this.workItemCount = this.workItems.length;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading work items:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
