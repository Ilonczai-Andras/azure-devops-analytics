import { Component } from '@angular/core';
import { CurrentSprintWiCount } from '../current-sprint-wi-count/current-sprint-wi-count';

@Component({
  selector: 'app-dashboard',
  imports: [CurrentSprintWiCount],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {

}
