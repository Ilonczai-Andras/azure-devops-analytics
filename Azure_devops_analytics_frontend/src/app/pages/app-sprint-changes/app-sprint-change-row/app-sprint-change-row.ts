import { Component, Input } from '@angular/core';
import { SprintWorkItemChange } from '../../../model/models';

@Component({
  selector: 'app-sprint-change-row',
  imports: [],
  templateUrl: './app-sprint-change-row.html',
  styleUrl: './app-sprint-change-row.css',
})
export class AppSprintChangeRow {
  @Input({ required: true }) item!: SprintWorkItemChange;
}
