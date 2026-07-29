import { Component, input } from '@angular/core';

export type BadgeVariant = 'success' | 'danger' | 'warning' | 'info' | 'neutral';

@Component({
  selector: 'app-badge',
  imports: [],
  templateUrl: './badge.html',
  styleUrl: './badge.scss',
})
export class BadgeComponent {
  readonly variant = input<BadgeVariant>('neutral');
}
