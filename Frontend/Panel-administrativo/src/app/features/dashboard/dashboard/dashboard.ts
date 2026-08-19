import { Component, inject, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { StatsService } from '../../../core/services/stats.service';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import type { StatsOverview, TopProduct } from '../../../core/models/index';

@Component({
  selector: 'app-dashboard',
  imports: [DecimalPipe, SkeletonComponent, BadgeComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent implements OnInit {
  private statsService = inject(StatsService);

  protected loading = signal(true);
  protected overview = signal<StatsOverview | null>(null);
  protected topViewed = signal<TopProduct[]>([]);
  protected topSellers = signal<TopProduct[]>([]);
  protected lowStock = signal<TopProduct[]>([]);

  ngOnInit(): void {
    this.statsService.getOverview().subscribe({
      next: data => { this.overview.set(data); this.loading.set(false); },
      error: () => this.loading.set(false),
    });

    this.statsService.getTopViewed().subscribe(data => this.topViewed.set(data));
    this.statsService.getTopSellers().subscribe(data => this.topSellers.set(data));
    this.statsService.getLowStock().subscribe(data => this.lowStock.set(data));
  }

  protected maxCount(items: TopProduct[]): number {
    return Math.max(...items.map(i => i.count), 1);
  }
}
