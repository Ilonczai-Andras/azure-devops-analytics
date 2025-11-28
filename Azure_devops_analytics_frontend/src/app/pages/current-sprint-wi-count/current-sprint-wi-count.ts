import { Component, ChangeDetectorRef, inject, OnInit } from '@angular/core';
import { ChartDataset, ChartOptions } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { WorkItemCount } from '../../model/models';
import { Azdoservice } from '../../service/azdoservice';

@Component({
  selector: 'app-current-sprint-wi-count',
  standalone: true,
  imports: [BaseChartDirective],
  templateUrl: './current-sprint-wi-count.html',
  styleUrl: './current-sprint-wi-count.css',
})
export class CurrentSprintWiCount implements OnInit {
  totalHours: Number = 0;
  isLoading: boolean = true;

  public pieChartData: ChartDataset[] = [];
  public pieChartLabels: string[] = [];
  public pieChartLegend = true;
  public pieChartPlugins = [];
  public pieChartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' } },
  };

  private cdr = inject(ChangeDetectorRef);

  constructor(private azdoService: Azdoservice) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.azdoService.getWiCount().subscribe({
      next: (data: WorkItemCount) => {
        this.processData(data);

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('API Hiba:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }

  processData(data: WorkItemCount) {
    this.totalHours = data.total;
    const labels = Object.keys(data.byType);
    const values = Object.values(data.byType);

    this.pieChartLabels = labels;
    this.pieChartData = [
      {
        data: values,
        backgroundColor: ['#6366F1', '#EF4444', '#10B981', '#F59E0B', '#8B5CF6'],
        hoverBackgroundColor: ['#4F46E5', '#DC2626', '#059669', '#D97706', '#7C3AED'],
      },
    ];
  }
}
