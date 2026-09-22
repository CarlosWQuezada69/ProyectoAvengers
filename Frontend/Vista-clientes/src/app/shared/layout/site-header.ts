import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

import { IconComponent } from '../icon/icon';
import { CartService } from '../../core/services/cart.service';
import { NAV_LINKS } from './nav-links';
import { SiteNavComponent } from './site-nav';

@Component({
  selector: 'app-site-header',
  standalone: true,
  imports: [IconComponent, RouterLink, ReactiveFormsModule, SiteNavComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './site-header.html',
  styleUrls: ['./site-header.scss']
})
export class SiteHeaderComponent {
  private readonly router = inject(Router);
  private readonly cart = inject(CartService);

  readonly navLinks = NAV_LINKS;
  readonly menuOpen = signal(false);
  readonly searchOpen = signal(false);

  readonly search = new FormControl<string>('');

  readonly cartCount = this.cart.count;

  constructor() {
    this.search.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => {
      // La búsqueda se ejecuta al enviar el formulario
    });
  }

  protected onSearchSubmit(): void {
    const term = this.search.value?.trim();
    this.router.navigate(['/productos'], { queryParams: term ? { q: term } : {} });
    this.searchOpen.set(false);
  }

  protected toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  protected toggleSearch(): void {
    this.searchOpen.update((open) => !open);
  }

  protected openCart(): void {
    this.cart.open();
  }
}