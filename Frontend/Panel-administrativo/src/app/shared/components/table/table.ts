import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';

export interface ColumnDef<T> {
  key: string;
  header: string;
  sortable?: boolean;
  cell?: (item: T) => string;
  template?: string;
}

@Component({
  selector: 'app-table',
  imports: [],
  templateUrl: './table.html',
  styleUrl: './table.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableComponent<T extends Record<string, unknown>> {
  readonly columns = input.required<ColumnDef<T>[]>();
  readonly data = input.required<T[]>();
  readonly page = input(1);
  readonly pageSize = input(20);
  readonly totalCount = input(0);
  readonly loading = input(false);
  readonly sortField = input<string | null>(null);
  readonly sortDir = input<'asc' | 'desc'>('asc');
  readonly sortChange = output<{ field: string; dir: 'asc' | 'desc' }>();
  readonly pageChange = output<number>();

  get totalPages(): number {
    return Math.ceil(this.totalCount() / this.pageSize()) || 1;
  }

  protected onSort(key: string): void {
    const dir = this.sortField() === key && this.sortDir() === 'asc' ? 'desc' : 'asc';
    this.sortChange.emit({ field: key, dir });
  }

  protected onPage(p: number): void {
    if (p >= 1 && p <= this.totalPages) {
      this.pageChange.emit(p);
    }
  }
}
