import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UsersService } from '../../../core/services/users.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { HasPermissionDirective } from '../../../shared/directives/has-permission.directive';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
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
  private confirmDialog = inject(ConfirmDialogService);

  protected users: User[] = [];
  protected loading = true;
  protected search = '';
  protected page = 1;
  protected pageSize = 20;
  protected totalCount = 0;

  ngOnInit(): void {
    this.loadUsers();
  }

  protected loadUsers(): void {
    this.loading = true;
    this.usersService.list({ search: this.search || undefined, page: this.page, pageSize: this.pageSize })
      .subscribe({
        next: res => { this.users = res.data; this.totalCount = res.totalCount; this.loading = false; },
        error: () => this.loading = false,
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
