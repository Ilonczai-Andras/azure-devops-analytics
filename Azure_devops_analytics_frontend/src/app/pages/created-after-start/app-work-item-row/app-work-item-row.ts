import { Component, Input } from '@angular/core';
import { WorkItem } from '../../../model/models';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-work-item-row',
  imports: [CommonModule],
  templateUrl: './app-work-item-row.html',
  styleUrl: './app-work-item-row.css',
})
export class AppWorkItemRow {
  @Input() item!: WorkItem;

  getAssignedName(): string {
    if (!this.item?.AssignedTo) {
      return 'Unassigned';
    }

    try {
      const assignedObj = JSON.parse(this.item.AssignedTo);
      return assignedObj.displayName || 'Unknown';
    } catch (e) {
      return this.item.AssignedTo;
    }
  }

  getStateClass(state: string): string {
    switch (state) {
      case 'In Progress': return 'state-active';
      case 'Done': return 'state-done';
      case 'To Do': return 'state-new';
      default: return '';
    }
  }
}
