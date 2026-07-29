import { Component, inject, HostListener, signal } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { DomSanitizer, type SafeHtml } from '@angular/platform-browser';
import { AuthService } from '../../core/services/auth.service';
import { BrandingService } from '../../core/services/branding.service';
import { BrandComponent } from '../components/brand/brand';
import { ToastContainerComponent } from '../components/toast/toast';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  permission?: string;
}

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, BrandComponent, ToastContainerComponent],
  templateUrl: './layout.html',
  styleUrl: './layout.scss',
})
export class LayoutComponent {
  private authService = inject(AuthService);
  private sanitizer = inject(DomSanitizer);
  protected branding = inject(BrandingService);

  protected sidebarOpen = signal(true);
  protected dropdownOpen = false;
  protected isMobile = signal(window.innerWidth < 769);

  readonly user = this.authService.user;
  readonly permissions = this.authService.permissions;

  @HostListener('window:resize')
  onResize() {
    this.isMobile.set(window.innerWidth < 769);
    if (!this.isMobile()) {
      this.sidebarOpen.set(true);
    }
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    this.dropdownOpen = false;
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (this.dropdownOpen && !target.closest('.dropdown')) {
      this.dropdownOpen = false;
    }
  }

  readonly menuItems: MenuItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard', permission: 'stats.view' },
    { label: 'Productos', icon: 'products', route: '/products', permission: 'products.view' },
    { label: 'Categorías', icon: 'categories', route: '/categories', permission: 'categories.create' },
    { label: 'Slider', icon: 'slider', route: '/slider', permission: 'slider.view' },
    { label: 'Configuración', icon: 'settings', route: '/settings', permission: 'settings.view' },
    { label: 'Usuarios', icon: 'users', route: '/users', permission: 'users.view' },
    { label: 'Roles', icon: 'roles', route: '/roles', permission: 'roles.view' },
    { label: 'Auditoría', icon: 'audit', route: '/audit', permission: 'audit.view' },
  ];

  get visibleMenuItems(): MenuItem[] {
    return this.menuItems.filter(
      item => !item.permission || this.permissions().includes(item.permission)
    );
  }

  private readonly SVG_ICONS: Record<string, string> = {
    dashboard: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/></svg>',
    products: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><polyline points="3.27 6.96 12 12.01 20.73 6.96"/><line x1="12" y1="22.08" x2="12" y2="12"/></svg>',
    categories: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z"/></svg>',
    slider: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="3" width="18" height="18" rx="2" ry="2"/><circle cx="8.5" cy="8.5" r="1.5"/><polyline points="21 15 16 10 5 21"/></svg>',
    settings: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"/><path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/></svg>',
    users: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>',
    roles: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>',
    audit: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"/><rect x="8" y="2" width="8" height="4" rx="1" ry="1"/><line x1="9" y1="13" x2="15" y2="13"/><line x1="9" y1="17" x2="13" y2="17"/></svg>',
  };

  protected getIcon(key: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(this.SVG_ICONS[key] || '');
  }

  protected toggleSidebar() {
    this.sidebarOpen.update(v => !v);
  }

  protected closeSidebar() {
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  toggleDropdown(): void {
    this.dropdownOpen = !this.dropdownOpen;
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.authService.clearTokens();
        window.location.href = '/auth/login';
      },
      error: () => {
        this.authService.clearTokens();
        window.location.href = '/auth/login';
      },
    });
  }
}
