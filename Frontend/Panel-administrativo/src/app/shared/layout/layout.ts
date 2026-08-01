import { Component, inject, HostListener, signal, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { BrandingService } from '../../core/services/branding.service';
import { BrandComponent } from '../components/brand/brand';
import { IconComponent } from '../components/icon/icon';
import { ConfirmDialogComponent } from '../components/confirm-dialog/confirm-dialog';
import { ToastContainerComponent } from '../components/toast/toast';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  permission?: string;
}

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, BrandComponent, IconComponent, ConfirmDialogComponent, ToastContainerComponent],
  templateUrl: './layout.html',
  styleUrl: './layout.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LayoutComponent {
  private authService = inject(AuthService);
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
    { label: 'Categorías', icon: 'categories', route: '/categories', permission: 'categories.view' },
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
