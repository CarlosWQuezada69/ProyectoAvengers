import { Component, inject, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../../core/services/users.service';
import { AuthService } from '../../../core/services/auth.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { HasPermissionDirective } from '../../../shared/directives/has-permission.directive';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import { ToastService } from '../../../shared/services/toast.service';
import type { User } from '../../../core/models/user';

@Component({
  selector: 'app-user-list',
  imports: [DatePipe, RouterLink, FormsModule, ButtonComponent, BadgeComponent, HasPermissionDirective],
  templateUrl: './user-list.html',
  styleUrl: './user-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserListComponent implements OnInit {
  private usersService = inject(UsersService);
  private authService = inject(AuthService);
  private confirmDialog = inject(ConfirmDialogService);
  private toast = inject(ToastService);

  protected users = signal<User[]>([]);
  protected loading = signal(true);
  protected search = '';
  protected page = 1;
  protected pageSize = 20;
  protected totalCount = signal(0);

  protected readonly currentUserId = this.authService.user;

  ngOnInit(): void {
    this.loadUsers();
  }

  protected loadUsers(): void {
    this.loading.set(true);
    this.usersService.list({ search: this.search || undefined, page: this.page, pageSize: this.pageSize })
      .subscribe({
        next: res => {
          this.users.set(res.data); this.totalCount.set(res.totalCount); this.loading.set(false);
          if (res.data.length === 0) {
            this.toast.info(this.search
              ? 'No se encontraron usuarios para tu búsqueda'
              : 'Aún no hay usuarios registrados');
          }
        },
        error: () => this.loading.set(false),
      });
  }

  protected onSearch(): void {
    this.page = 1;
    this.loadUsers();
  }

  protected onPage(p: number): void {
    this.page = p;
    this.loadUsers();
  }

  protected async deleteUser(id: string): Promise<void> {
    const confirmed = await this.confirmDialog.confirm('¿Desactivar este usuario?');
    if (!confirmed) return;
    this.usersService.delete(id).subscribe(() => this.loadUsers());
  }
}
