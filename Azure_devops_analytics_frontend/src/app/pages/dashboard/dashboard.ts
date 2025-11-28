import { Component } from '@angular/core';
import { CurrentSprintWiCount } from '../current-sprint-wi-count/current-sprint-wi-count';
import { AppSprintChanges } from '../app-sprint-changes/app-sprint-changes';
import { CreatedAfterStart } from '../created-after-start/created-after-start';
import { SprintCapacity } from '../sprint-capacity/sprint-capacity';

@Component({
  selector: 'app-dashboard',
  imports: [CurrentSprintWiCount, AppSprintChanges, CreatedAfterStart, SprintCapacity],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {

}
