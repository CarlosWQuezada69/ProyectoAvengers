import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
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

  protected loading = true;
  protected overview: StatsOverview | null = null;
  protected topViewed: TopProduct[] = [];
  protected topSellers: TopProduct[] = [];
  protected lowStock: TopProduct[] = [];

  ngOnInit(): void {
    this.statsService.getOverview().subscribe({
      next: data => { this.overview = data; this.loading = false; },
      error: () => this.loading = false,
    });

    this.statsService.getTopViewed().subscribe(data => this.topViewed = data);
    this.statsService.getTopSellers().subscribe(data => this.topSellers = data);
    this.statsService.getLowStock().subscribe(data => this.lowStock = data);
  }

  protected maxCount(items: TopProduct[]): number {
    return Math.max(...items.map(i => i.count), 1);
  }
}
