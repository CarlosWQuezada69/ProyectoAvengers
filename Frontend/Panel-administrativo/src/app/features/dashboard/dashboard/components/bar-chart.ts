import { Component, computed, input } from '@angular/core';

export interface DailyBar {
  label: string;
  productViews: number;
  pageViews: number;
}

@Component({
  selector: 'app-bar-chart',
  standalone: true,
  imports: [],
  templateUrl: './bar-chart.html',
  styleUrl: './bar-chart.scss',
})
export class BarChartComponent {
  readonly data = input<DailyBar[]>([]);

  protected readonly max = computed(() =>
    Math.max(...this.data().map((d) => d.productViews + d.pageViews), 1),
  );

  protected readonly hasData = computed(() => this.data().some((d) => d.productViews > 0 || d.pageViews > 0));
}