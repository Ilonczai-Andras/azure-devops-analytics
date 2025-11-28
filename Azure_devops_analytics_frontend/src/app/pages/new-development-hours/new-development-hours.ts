import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { DevelopmentHoursResponse, DevelopmentMember } from '../../model/models';
import { ChartConfiguration, ChartOptions, ChartType } from 'chart.js';
import { Azdoservice } from '../../service/azdoservice';

@Component({
  selector: 'app-new-development-hours',
  imports: [BaseChartDirective],
  templateUrl: './new-development-hours.html',
  styleUrl: './new-development-hours.css',
})
export class NewDevelopmentHours implements OnInit {
  isLoading: boolean = true;
  totalHours: number = 0;

  members: DevelopmentMember[] = [];

  chartType: ChartType = 'bar';

  chartData: ChartConfiguration['data'] = {
  labels: [],
  datasets: [
    {
      label: 'Development Hours',
      data: [],
      backgroundColor: '#23a023ff',
      borderColor: '#000000ff',
      hoverBackgroundColor: '#9dff00ff',
      borderWidth: 1
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
      tooltip: {
        callbacks: {
          label: (context) => {
            const member = this.members[context.dataIndex];

            if (!member) return 'Nincs adat';

            return [
              `Fejlesztési órák: ${member.developmentHours}`,
              `Work Item szám: ${member.workItemCount}`,
            ];
          },
        },
      },
    },
  };

  constructor(private azdoservice: Azdoservice, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadDevelopmentHours();
  }

  private loadDevelopmentHours(): void {
    this.isLoading = true;

    this.azdoservice.getDevelopmentHours().subscribe({
      next: (data: DevelopmentHoursResponse) => {
        this.totalHours = data.totalDevelopmentHours;

        this.members = data.members;

        this.chartData.labels = data.members.map((m) => m.name);
        this.chartData.datasets[0].data = data.members.map((m) => m.developmentHours);

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Development hours betöltési hiba:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
    });
  }
}