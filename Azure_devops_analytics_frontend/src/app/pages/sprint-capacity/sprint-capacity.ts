import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ChartConfiguration, ChartOptions, ChartType } from 'chart.js';
import { TeamCapacity } from '../../model/models';
import { Azdoservice } from '../../service/azdoservice';
import { BaseChartDirective } from 'ng2-charts';

@Component({
  selector: 'app-sprint-capacity',
  imports: [BaseChartDirective],
  templateUrl: './sprint-capacity.html',
  styleUrl: './sprint-capacity.css',
})
export class SprintCapacity implements OnInit {

  isLoading: boolean = true;
  totalHours: number = 0;
  averageEffectiveDays: number = 0;

  public chartType: ChartType = 'bar';

  public chartOptions: ChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y',
    plugins: {
      legend: {
        display: false
      }
  }};

  public chartData: ChartConfiguration['data'] = {
    datasets: [],
    labels: []
  };

  constructor(private azdoService: Azdoservice, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadCapacityData();
  }

  loadCapacityData() {
    this.azdoService.getTeamCapacity().subscribe({
      next: (data: TeamCapacity) => {

        this.totalHours = data.totalRealWorkHours;

        const memberNamesWithDays = data.members.map(
          m => `${m.name} (${m.effectiveDays} nap)`
        );

        const memberHours = data.members.map(m => m.hours);

        const totalEffectiveDays = data.members.reduce(
          (acc, m) => acc + m.effectiveDays, 0
        );

        this.averageEffectiveDays = data.members.length > 0
          ? totalEffectiveDays / data.members.length
          : 0;

        this.chartData = {
          labels: memberNamesWithDays,
          datasets: [
            {
              data: memberHours,
              label: 'Kapacitás (óra)',
              backgroundColor: '#1b998b',
              borderColor: '#1b998b',
              hoverBackgroundColor: '#178a7d',
              borderWidth: 1
            }
          ]
        };

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Hiba a kapacitás adatok betöltésekor:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
