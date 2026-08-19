import { Component, inject, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { DatePipe, JsonPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditService } from '../../../core/services/audit.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { ToastService } from '../../../shared/services/toast.service';
import type { AuditLog } from '../../../core/models/index';

@Component({
  selector: 'app-audit-log',
  imports: [DatePipe, JsonPipe, FormsModule, ButtonComponent, BadgeComponent],
  templateUrl: './audit-log.html',
  styleUrl: './audit-log.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuditLogComponent implements OnInit {
  private auditService = inject(AuditService);
  private toast = inject(ToastService);

  protected logs = signal<AuditLog[]>([]);
  protected loading = true;
  protected entityName = '';
  protected from = '';
  protected to = '';
  protected page = 1;
  protected pageSize = 20;
  protected totalCount = 0;
  protected totalPages = signal(0);
  protected expandedId: string | null = null;

  ngOnInit(): void {
    this.loadLogs();
  }

  protected loadLogs(): void {
    this.loading = true;
    this.auditService.list({
      entityName: this.entityName,
      from: this.from,
      to: this.to,
      page: this.page,
      pageSize: this.pageSize,
    }).subscribe({
      next: res => { this.logs.set(res.data); this.totalCount = res.totalCount; this.totalPages.set(res.totalPages); this.loading = false;
        if (res.data.length === 0) this.toast.info('Aún no hay registros de auditoría');
      },
      error: () => this.loading = false,
    });
  }

  protected toggleExpand(id: string): void {
    this.expandedId = this.expandedId === id ? null : id;
  }

  protected onFilter(): void {
    this.page = 1;
    this.loadLogs();
  }

  protected onPage(p: number): void {
    this.page = p;
    this.loadLogs();
  }

  protected actionVariant(action: string): 'info' | 'success' | 'warning' | 'danger' {
    switch (action) {
      case 'CREATE': return 'success';
      case 'UPDATE': return 'info';
      case 'DELETE': return 'danger';
      default: return 'warning';
    }
  }
}
