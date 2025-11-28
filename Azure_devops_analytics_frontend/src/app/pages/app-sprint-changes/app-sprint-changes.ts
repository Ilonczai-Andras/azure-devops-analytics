import { ChangeDetectorRef, Component, OnInit } from '@angular/core'; // inject nem kell ide
import { AppSprintChangeRow } from './app-sprint-change-row/app-sprint-change-row';
import { SprintWorkItemChange } from '../../model/models';
import { Azdoservice } from '../../service/azdoservice';

@Component({
  selector: 'app-sprint-changes',
  imports: [AppSprintChangeRow],
  templateUrl: './app-sprint-changes.html',
  styleUrl: './app-sprint-changes.css',
})
export class AppSprintChanges implements OnInit {
  isLoading: boolean = true;
  totalRemovedItemCount: number = 0;
  sprintChangesList: SprintWorkItemChange[] = [];

  constructor(private azdoService: Azdoservice, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadData();
  }

  private loadData() {
    this.azdoService.getSprintChanges().subscribe({
      next: (data) => {
        this.sprintChangesList = data;
        this.totalRemovedItemCount = this.sprintChangesList.length;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Hiba történt az adatok lekérésekor:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }
}
