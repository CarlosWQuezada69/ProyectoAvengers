import { Component, inject, OnInit, ChangeDetectionStrategy, computed, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { StatsService } from '../../../core/services/stats.service';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { DonutChartComponent, type DonutSlice } from './components/donut-chart';
import { BarChartComponent, type DailyBar } from './components/bar-chart';
import type { DailyViewsStat, PageViewsStat, StatsOverview, TopProduct } from '../../../core/models/index';
import { RevealDirective } from '../../../shared/directives/reveal.directive';

@Component({
  selector: 'app-dashboard',
  imports: [DecimalPipe, SkeletonComponent, BadgeComponent, DonutChartComponent, BarChartComponent, RevealDirective],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent implements OnInit {
  private statsService = inject(StatsService);

  protected loading = signal(true);
  protected overview = signal<StatsOverview | null>(null);
  protected topViewed = signal<TopProduct[]>([]);
  protected lowStock = signal<TopProduct[]>([]);
  protected dailyViews = signal<DailyViewsStat[]>([]);
  protected pageViews = signal<PageViewsStat[]>([]);

  protected readonly donutSlices = computed<DonutSlice[]>(() =>
    this.pageViews().map((p) => ({ label: p.pageLabel, value: p.count })),
  );

  protected readonly barData = computed<DailyBar[]>(() =>
    this.dailyViews().map((d) => ({
      label: d.label,
      productViews: d.productViews,
      pageViews: d.pageViews,
    })),
  );

  ngOnInit(): void {
    this.statsService.getOverview().subscribe({
      next: data => { this.overview.set(data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });

    this.statsService.getTopViewed().subscribe(data => this.topViewed.set(data));
    this.statsService.getLowStock().subscribe(data => this.lowStock.set(data));
    this.statsService.getDailyViews(7).subscribe(data => this.dailyViews.set(data));
    this.statsService.getPageViews().subscribe(data => this.pageViews.set(data));
  }

  protected maxCount(items: TopProduct[]): number {
    return Math.max(...items.map(i => i.count), 1);
  }
}