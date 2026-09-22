import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type ProductTheme = 'shield' | 'reactor' | 'mjolnir' | 'gems' | 'generic';

const THEME_KEYWORDS: Record<ProductTheme, string[]> = {
  shield: ['capit', 'américa', 'america', 'escudo'],
  reactor: ['arc', 'iron', 'reactor'],
  mjolnir: ['thor', 'martillo', 'mjolnir'],
  gems: ['infinito', 'gemas', 'gauntlet', 'guante'],
  generic: []
};

function themeFor(name: string): ProductTheme {
  const lower = name.toLowerCase();
  const entry = (Object.entries(THEME_KEYWORDS) as [ProductTheme, string[]][]).find(([, keys]) =>
    keys.some((k) => lower.includes(k))
  );
  return entry ? entry[0] : 'generic';
}

let uid = 0;

@Component({
  selector: 'app-product-image-fallback',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (imageUrl(); as url) {
      <img [src]="url" [alt]="alt()" loading="lazy" />
    } @else {
      <svg class="placeholder" viewBox="0 0 400 400" role="img" [attr.aria-label]="alt()">
        <defs>
          <radialGradient [attr.id]="gradientId" cx="50%" cy="40%" r="75%">
            <stop offset="0%" stop-color="#2a2310" />
            <stop offset="100%" stop-color="#121214" />
          </radialGradient>
        </defs>
        <rect width="400" height="400" [attr.fill]="'url(#' + gradientId + ')'" />
        @switch (theme()) {
          @case ('shield') {
            <path d="M200 70l90 40v70c0 62-40 96-90 118-50-22-90-56-90-118V110z" fill="none" stroke="#f0c419" stroke-width="8" />
            <path d="M200 96l58 26v55c0 41-24 65-58 82-34-17-58-41-58-82v-55z" fill="none" stroke="#e74c3c" stroke-width="6" opacity="0.85" />
            <path d="M200 150l22 42 47 5-34 31 9 47-44-24-44 24 9-47-34-31 47-5z" fill="#f0c419" stroke="none" opacity="0.9" />
          }
          @case ('reactor') {
            <circle cx="200" cy="200" r="110" fill="none" stroke="#f0c419" stroke-width="6" opacity="0.9" />
            <circle cx="200" cy="200" r="78" fill="none" stroke="#e8b923" stroke-width="3" opacity="0.6" />
            <path d="M200 122l30 78-30 78-30-78z" fill="none" stroke="#f0c419" stroke-width="8" opacity="0.9" />
            <circle cx="200" cy="200" r="26" fill="none" stroke="#ffffff" stroke-width="6" opacity="0.85" />
          }
          @case ('mjolnir') {
            <rect x="160" y="120" width="80" height="70" rx="8" fill="none" stroke="#f0c419" stroke-width="8" />
            <rect x="178" y="190" width="44" height="90" fill="none" stroke="#f0c419" stroke-width="8" />
            <path d="M188 260h24M188 280h12" stroke="#f0c419" stroke-width="8" stroke-linecap="round" />
            <path d="M120 130h160M120 180h160" stroke="#e8b923" stroke-width="3" opacity="0.5" stroke-dasharray="6 8" />
          }
          @case ('gems') {
            <rect x="150" y="110" width="100" height="180" rx="12" fill="none" stroke="#f0c419" stroke-width="8" />
            <g>
              <circle cx="200" cy="165" r="12" fill="#4a7bdb" />
              <circle cx="200" cy="200" r="12" fill="#d33b3b" />
              <circle cx="200" cy="235" r="12" fill="#3bbf8a" />
              <circle cx="185" cy="183" r="9" fill="#f5d77a" opacity="0.95" />
              <circle cx="215" cy="183" r="9" fill="#5e4fbf" opacity="0.95" />
              <circle cx="200" cy="218" r="9" fill="#e8b923" opacity="0.95" />
            </g>
          }
          @default {
            <path d="M200 120c-40 0-66 26-66 62 0 46 44 88 56 102 2 2 18 2 20 0 12-14 56-56 56-102 0-36-26-62-66-62z" fill="none" stroke="#f0c419" stroke-width="8" />
            <circle cx="200" cy="182" r="26" fill="none" stroke="#f0c419" stroke-width="6" opacity="0.9" />
            <circle cx="200" cy="182" r="10" fill="#f0c419" opacity="0.85" />
          }
        }
      </svg>
    }
  `,
  styles: `
    :host {
      display: block;
      width: 100%;
      height: 100%;
      background: #161618;
    }
    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
    .placeholder {
      display: block;
      width: 100%;
      height: 100%;
    }
  `
})
export class ProductImageFallbackComponent {
  readonly product = input.required<{ name: string }>();
  readonly imageUrl = input<string | null>(null);
  readonly alt = input<string>('');

  readonly theme = computed(() => themeFor(this.product().name));
  readonly gradientId = `ph-gradient-${++uid}`;
}