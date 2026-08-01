import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductsService } from '../../../core/services/products.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { HasPermissionDirective } from '../../../shared/directives/has-permission.directive';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import type { Product } from '../../../core/models/product';

@Component({
  selector: 'app-product-list',
  imports: [DatePipe, RouterLink, FormsModule, ButtonComponent, BadgeComponent, HasPermissionDirective],
  templateUrl: './product-list.html',
  styleUrl: './product-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductListComponent implements OnInit {
  private productsService = inject(ProductsService);
  private confirmDialog = inject(ConfirmDialogService);

  protected products: Product[] = [];
  protected loading = true;
  protected search = '';
  protected page = 1;
  protected pageSize = 20;
  protected totalCount = 0;
  protected totalPages = 0;
  protected sortBy = 'createdAt';
  protected sortDir: 'asc' | 'desc' = 'desc';

  ngOnInit(): void {
    this.loadProducts();
  }

  protected loadProducts(): void {
    this.loading = true;
    this.productsService.list({
      search: this.search || undefined,
      page: this.page,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDir: this.sortDir,
    })      .subscribe({
        next: res => {
          this.products = res.data;
          this.totalCount = res.totalCount;
          this.totalPages = res.totalPages;
          this.loading = false;
        },
        error: () => this.loading = false,
      });
  }

  protected onSearch(): void {
    this.page = 1;
    this.loadProducts();
  }

  protected onPage(p: number): void {
    if (p >= 1 && p <= this.totalPages) {
      this.page = p;
      this.loadProducts();
    }
  }

  protected async deleteProduct(id: string): Promise<void> {
    const confirmed = await this.confirmDialog.confirm('¿Eliminar este producto?');
    if (!confirmed) return;
    this.productsService.delete(id).subscribe(() => this.loadProducts());
  }

  protected onSort(field: string): void {
    if (this.sortBy === field) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortBy = field;
      this.sortDir = 'asc';
    }
    this.loadProducts();
  }
}
