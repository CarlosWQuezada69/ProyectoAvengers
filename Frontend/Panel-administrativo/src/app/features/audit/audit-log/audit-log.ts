import { Component, inject, OnInit } from '@angular/core';
import { DatePipe, JsonPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditService } from '../../../core/services/audit.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import type { AuditLog } from '../../../core/models/index';

@Component({
  selector: 'app-audit-log',
  imports: [DatePipe, JsonPipe, FormsModule, ButtonComponent, BadgeComponent],
  templateUrl: './audit-log.html',
  styleUrl: './audit-log.scss',
})
export class AuditLogComponent implements OnInit {
  private auditService = inject(AuditService);

  protected logs: AuditLog[] = [];
  protected loading = true;
  protected entityName = '';
  protected from = '';
  protected to = '';
  protected page = 1;
  protected pageSize = 20;
  protected totalCount = 0;
  protected totalPages = 0;
  protected expandedId: string | null = null;

  ngOnInit(): void {
    this.loadLogs();
  }

  protected loadLogs(): void {
    this.loading = true;
    this.auditService.list({
      entityName: this.entityName || undefined,
      from: this.from || undefined,
      to: this.to || undefined,
      page: this.page,
      pageSize: this.pageSize,
    }).subscribe({
      next: res => { this.logs = res.data; this.totalCount = res.totalCount; this.totalPages = res.totalPages; this.loading = false; },
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
