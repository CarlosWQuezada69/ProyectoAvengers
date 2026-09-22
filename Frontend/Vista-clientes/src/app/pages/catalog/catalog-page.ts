import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { CatalogService } from '../../core/services/catalog.service';
import { CartService } from '../../core/services/cart.service';
import { CategoryDto } from '../../core/models/category';
import { PagedResult } from '../../core/models/paged-result';
import { ProductListDto } from '../../core/models/product';
import { IconComponent } from '../../shared/icon/icon';
import { ProductCardComponent } from '../../shared/components/product-card/product-card';

const PAGE_SIZE = 12;
const SORT_OPTIONS = [
  { value: 'newest', label: 'Más recientes' },
  { value: 'price_asc', label: 'Precio: menor a mayor' },
  { value: 'price_desc', label: 'Precio: mayor a menor' },
  { value: 'name_asc', label: 'Nombre: A–Z' },
  { value: 'name_desc', label: 'Nombre: Z–A' }
] as const;

function flatten(categories: CategoryDto[], result: CategoryDto[] = []): CategoryDto[] {
  for (const category of categories) {
    result.push(category);
    if (category.children.length > 0) {
      flatten(category.children, result);
    }
  }
  return result;
}

@Component({
  selector: 'app-catalog-page',
  standalone: true,
  imports: [IconComponent, ProductCardComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './catalog-page.html',
  styleUrls: ['./catalog-page.scss']
})
export class CatalogPageComponent {
  private readonly catalog = inject(CatalogService);
  private readonly cart = inject(CartService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly sortOptions = SORT_OPTIONS;

  readonly search = signal<string>('');
  readonly categoryId = signal<string>('');
  readonly sort = signal<string>('newest');
  readonly onlyAvailable = signal(false);
  readonly page = signal(1);

  readonly categories = signal<CategoryDto[]>([]);
  readonly flatCategories = computed(() => flatten(this.categories()));

  readonly result = signal<PagedResult<ProductListDto> | null>(null);
  readonly loading = signal(true);

  readonly totalPages = computed(() => this.result()?.totalPages ?? 1);
  readonly totalCount = computed(() => this.result()?.totalCount ?? 0);
  readonly rangeStart = computed(() =>
    this.result() ? (this.result()!.page - 1) * this.result()!.pageSize + 1 : 0
  );
  readonly rangeEnd = computed(() =>
    this.result() ? Math.min(this.result()!.page * this.result()!.pageSize, this.result()!.totalCount) : 0
  );
  readonly pages = computed(() => this.buildPages(this.page(), this.totalPages()));

  private requestedPageRequest = 0;

  constructor() {
    this.catalog.getCategories().subscribe((cats) => this.categories.set(cats));

    this.route.queryParams.pipe(takeUntilDestroyed()).subscribe((params) => {
      const nextSearch = (params['q'] as string | undefined) ?? '';
      const nextCategory = (params['cat'] as string | undefined) ?? '';
      const nextSort = (params['sort'] as string | undefined) ?? 'newest';
      const nextAvailable = params['available'] === 'true';
      const nextPage = Number(params['page'] ?? 1);

      if (nextSearch !== this.search()) this.search.set(nextSearch);
      if (nextCategory !== this.categoryId()) this.categoryId.set(nextCategory);
      if (nextSort !== this.sort()) this.sort.set(nextSort);
      if (nextAvailable !== this.onlyAvailable()) this.onlyAvailable.set(nextAvailable);
      if (nextPage !== this.page()) this.page.set(nextPage);

      this.load();
    });
  }

  protected onSearch(): void {
    this.applyQuery({ page: 1 });
  }

  protected onCategoryChange(categoryId: string): void {
    this.applyQuery({ cat: categoryId || undefined, page: 1 });
  }

  protected onSortChange(sort: string): void {
    this.applyQuery({ sort: sort === 'newest' ? undefined : sort, page: 1 });
  }

  protected onAvailabilityChange(checked: boolean): void {
    this.applyQuery({ available: checked ? 'true' : undefined, page: 1 });
  }

  protected goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) return;
    this.applyQuery({ page });
  }

  protected addToCart(product: ProductListDto): void {
    this.cart.add(product);
  }

  protected resetFilters(): void {
    this.router.navigate(['/productos'], { queryParams: {} });
  }

  private applyQuery(patch: Record<string, string | number | undefined>): void {
    const base = {
      q: this.search() || undefined,
      cat: this.categoryId() || undefined,
      sort: this.sort() === 'newest' ? undefined : this.sort(),
      available: this.onlyAvailable() || undefined,
      page: 1
    };

    this.router.navigate(['/productos'], {
      queryParams: { ...base, ...patch },
      queryParamsHandling: 'merge'
    });
  }

  private load(): void {
    const reqId = ++this.requestedPageRequest;
    this.loading.set(true);

    this.catalog
      .getProducts({
        search: this.search() || null,
        categoryId: this.categoryId() || null,
        onlyAvailable: this.onlyAvailable() ? true : null,
        sort: this.sort() === 'newest' ? null : this.sort(),
        page: this.page(),
        pageSize: PAGE_SIZE
      })
      .subscribe({
        next: (result) => {
          if (reqId !== this.requestedPageRequest) return;
          this.result.set(result);
          this.loading.set(false);
        },
        error: () => {
          if (reqId !== this.requestedPageRequest) return;
          this.result.set(null);
          this.loading.set(false);
        }
      });
  }

  private buildPages(current: number, total: number): number[] {
    if (total <= 1) return [];
    const delta = 2;
    const start = Math.max(current - delta, 1);
    const end = Math.min(current + delta, total);
    const pages: number[] = [];
    for (let i = start; i <= end; i += 1) pages.push(i);
    if (start > 1) pages.unshift(1);
    if (end < total) pages.push(total);
    return pages;
  }
}