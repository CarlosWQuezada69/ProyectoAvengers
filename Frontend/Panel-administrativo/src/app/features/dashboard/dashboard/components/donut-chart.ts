import { Component, computed, input } from '@angular/core';
import { DecimalPipe } from '@angular/common';

export interface DonutSlice {
  label: string;
  value: number;
}

const DONUT_PALETTE = ['#E3C47C', '#C9A24B', '#9A7B2F', '#7C6238', '#6E7480', '#8A9BA8', '#4A4A55', '#2E2E36'];

@Component({
  selector: 'app-donut-chart',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './donut-chart.html',
  styleUrl: './donut-chart.scss',
})
export class DonutChartComponent {
  readonly data = input<DonutSlice[]>([]);
  readonly size = input(180);
  readonly thickness = input(22);

  protected readonly palette = DONUT_PALETTE;

  protected readonly total = computed(() => this.data().reduce((acc, s) => acc + s.value, 0));
  protected readonly radius = computed(() => (this.size() - this.thickness()) / 2);
  protected readonly circumference = computed(() => 2 * Math.PI * this.radius());

  protected readonly segments = computed(() => {
    const total = this.total();
    if (total <= 0) return [];

    const circ = this.circumference();
    let accumulated = 0;

    return this.data().map((slice, index) => {
      const length = (slice.value / total) * circ;
      const segment = {
        color: DONUT_PALETTE[index % DONUT_PALETTE.length],
        radius: this.radius(),
        length,
        offset: -accumulated,
        value: slice.value,
      };
      accumulated += length;
      return segment;
    });
  });

  protected percent(value: number): number {
    const total = this.total();
    return total > 0 ? (value / total) * 100 : 0;
  }
}