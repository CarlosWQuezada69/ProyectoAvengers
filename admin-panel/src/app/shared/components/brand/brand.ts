import { Component, inject, input } from '@angular/core';
import { BrandingService } from '../../../core/services/branding.service';

@Component({
  selector: 'app-brand',
  imports: [],
  template: `
    @if (logoUrl(); as url) {
      <img class="brand-logo" [src]="url" [alt]="businessName()" />
    } @else {
      <svg class="brand-logo" viewBox="0 0 32 32" fill="none" stroke="currentColor" stroke-width="2.5">
        <polygon points="16 2 30 12 30 26 16 30 2 26 2 12" stroke-linejoin="round"/>
        <line x1="12" y1="14" x2="20" y2="14"/>
        <line x1="12" y1="18" x2="20" y2="18"/>
        <line x1="14" y1="22" x2="18" y2="22"/>
      </svg>
    }
    @if (showText()) {
      <span class="brand-name">{{ businessName() }}</span>
    }
  `,
  styles: [`
    :host {
      display: inline-flex;
      align-items: center;
      gap: 10px;
    }
    .brand-logo {
      width: 32px;
      height: 32px;
      object-fit: contain;
      flex-shrink: 0;
    }
    .brand-name {
      font-size: 16px;
      font-weight: 700;
      color: var(--text-primary);
      white-space: nowrap;
    }
  `],
})
export class BrandComponent {
  private branding = inject(BrandingService);

  protected logoUrl = this.branding.logoUrl;
  protected businessName = this.branding.businessName;

  showText = input(true);
}
