import { Component } from '@angular/core';
import { ChartOptions } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

@Component({
  selector: 'app-current-sprint-wi-count',
  imports: [BaseChartDirective],
  templateUrl: './current-sprint-wi-count.html',
  styleUrl: './current-sprint-wi-count.css',
})
export class CurrentSprintWiCount {
  totalHours: Number = 0;
  isLoading: boolean = true;

  public pieChartOptions: ChartOptions = {
  responsive: true,
  maintainAspectRatio: false, // Important for fitting into grid cards
  plugins: {
    legend: {
      position: 'bottom', // Looks better in cards
    }
  }
};
}
