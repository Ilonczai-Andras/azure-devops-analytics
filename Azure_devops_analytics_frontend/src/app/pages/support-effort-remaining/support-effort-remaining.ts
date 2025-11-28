import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { SupportResponse } from '../../model/models';
import { ChartConfiguration, ChartOptions, ChartType } from 'chart.js';
import { Azdoservice } from '../../service/azdoservice';

@Component({
  selector: 'app-support-effort-remaining',
  imports: [BaseChartDirective],
  templateUrl: './support-effort-remaining.html',
  styleUrl: './support-effort-remaining.css',
})
export class SupportEffortRemaining implements OnInit {
  isLoading: boolean = true;

  stats = {
    effort: 0,
    remaining: 0,
  };

  supportData!: SupportResponse;

  chartType: ChartType = 'bar';

  chartData: ChartConfiguration['data'] = {
    labels: [],
    datasets: [
      {
        label: 'Effort',
        data: [],
      },
      {
        label: 'Remaining Work',
        data: [],
      },
    ],
  };

  public chartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y',
    plugins: {
      legend: {
        display: false,
      },
    },
  };

  constructor(private azdoservice: Azdoservice, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadSupportHours();
  }

  private loadSupportHours(): void {
    this.azdoservice.getSupportHours().subscribe({
      next: (data) => {
        this.supportData = data;

        this.stats.effort = data.totalEffort;
        this.stats.remaining = data.totalRemaining;

        this.chartData.labels = data.members.map((m) => m.name);

        this.chartData.datasets[0].data = data.members.map((m) => m.effort);
        this.chartData.datasets[1].data = data.members.map((m) => m.remainingWork);

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Support hours loading error:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }
}
