import { Component, inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductsService } from '../../../core/services/products.service';
import { CategoriesService } from '../../../core/services/categories.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { InputComponent } from '../../../shared/components/input/input';
import { SelectComponent } from '../../../shared/components/select/select';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { ModalComponent } from '../../../shared/components/modal/modal';
import { UploaderComponent } from '../../../shared/components/uploader/uploader';
import { ToastService } from '../../../shared/services/toast.service';
import { HasPermissionDirective } from '../../../shared/directives/has-permission.directive';
import type { Product, ProductImage, ProductRestriction, RestrictionType } from '../../../core/models/product';
import type { Category } from '../../../core/models/category';

@Component({
  selector: 'app-product-form',
  imports: [ReactiveFormsModule, RouterLink, ButtonComponent, InputComponent, SelectComponent, BadgeComponent, ModalComponent, UploaderComponent, HasPermissionDirective],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss',
})
export class ProductFormComponent implements OnInit {
  private productsService = inject(ProductsService);
  private categoriesService = inject(CategoriesService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  protected editMode = false;
  protected productId: string | null = null;
  protected loading = false;
  protected saving = false;
  protected categories: Category[] = [];
  protected images: ProductImage[] = [];
  protected restrictions: ProductRestriction[] = [];
  protected restrictionModalOpen = false;
  protected editingRestriction: ProductRestriction | null = null;

  protected form = new FormGroup({
    name: new FormControl('', [Validators.required]),
    sku: new FormControl('', [Validators.required]),
    slug: new FormControl('', [Validators.required]),
    description: new FormControl(''),
    categoryId: new FormControl(''),
    price: new FormControl(0, [Validators.required, Validators.min(0)]),
    compareAtPrice: new FormControl<number | null>(null),
    stock: new FormControl(0, [Validators.min(0)]),
    isFeatured: new FormControl(false),
    isActive: new FormControl(true),
  });

  protected restrictionForm = new FormGroup({
    restrictionType: new FormControl<RestrictionType>('AGE_MIN', [Validators.required]),
    config: new FormControl('{}'),
    startsAt: new FormControl(''),
    endsAt: new FormControl(''),
    isActive: new FormControl(true),
  });

  ngOnInit(): void {
    this.productId = this.route.snapshot.paramMap.get('id');
    this.editMode = !!this.productId;

    this.categoriesService.list().subscribe(res => {
      this.categories = Array.isArray(res) ? res : (res as any).data ?? [];
    });

    if (this.editMode && this.productId) {
      this.loading = true;
      this.productsService.getById(this.productId).subscribe({
        next: product => {
          this.form.patchValue(product);
          this.images = product.images ?? [];
          this.restrictions = product.restrictions ?? [];
          this.loading = false;
        },
        error: () => this.loading = false,
      });
    }
  }

  protected onSubmit(): void {
    if (this.form.invalid) return;
    this.saving = true;

    const data = this.form.value;

    if (this.editMode && this.productId) {
      this.productsService.update(this.productId, data as any).subscribe({
        next: () => {
          this.toast.show('Producto actualizado', 'success');
          this.saving = false;
        },
        error: () => { this.saving = false; },
      });
    } else {
      this.productsService.create(data as any).subscribe({
        next: (res) => {
          this.toast.show('Producto creado', 'success');
          this.router.navigate(['/products', res.id]);
        },
        error: () => { this.saving = false; },
      });
    }
  }

  protected onImagesUploaded(files: File[]): void {
    if (!this.productId) return;
    for (const file of files) {
      this.productsService.uploadImage(this.productId, file).subscribe(img => {
        this.images = [...this.images, img];
      });
    }
  }

  protected removeImage(imageId: string): void {
    if (!this.productId) return;
    this.productsService.deleteImage(this.productId, imageId).subscribe(() => {
      this.images = this.images.filter(i => i.id !== imageId);
    });
  }

  protected openRestrictionModal(r?: ProductRestriction): void {
    this.editingRestriction = r ?? null;
    if (r) {
      this.restrictionForm.patchValue({
        ...r,
        startsAt: r.startsAt?.split('T')[0] ?? '',
        endsAt: r.endsAt?.split('T')[0] ?? '',
        config: JSON.stringify(r.config),
      });
    } else {
      this.restrictionForm.reset({ restrictionType: 'AGE_MIN', config: '{}', isActive: true });
    }
    this.restrictionModalOpen = true;
  }

  protected saveRestriction(): void {
    if (!this.productId || this.restrictionForm.invalid) return;
    const data = { ...this.restrictionForm.value, config: JSON.parse(this.restrictionForm.value.config || '{}') };

    if (this.editingRestriction) {
      this.productsService.updateRestriction(this.productId, this.editingRestriction.id, data as any).subscribe({
        next: (r) => {
          this.restrictions = this.restrictions.map(rest => rest.id === r.id ? r : rest);
          this.restrictionModalOpen = false;
          this.toast.show('Restricción actualizada', 'success');
        },
      });
    } else {
      this.productsService.addRestriction(this.productId, data as any).subscribe({
        next: (r) => {
          this.restrictions = [...this.restrictions, r];
          this.restrictionModalOpen = false;
          this.toast.show('Restricción agregada', 'success');
        },
      });
    }
  }

  protected deleteRestriction(restrictionId: string): void {
    if (!this.productId) return;
    this.productsService.deleteRestriction(this.productId, restrictionId).subscribe(() => {
      this.restrictions = this.restrictions.filter(r => r.id !== restrictionId);
      this.toast.show('Restricción eliminada', 'success');
    });
  }

  protected restrictionTypeLabel(type: RestrictionType): string {
    const labels: Record<RestrictionType, string> = {
      AGE_MIN: 'Edad mínima',
      PURCHASE_LIMIT_USER: 'Límite por cliente',
      PURCHASE_LIMIT_ORDER: 'Límite por pedido',
      AVAILABILITY_WINDOW: 'Ventana de disponibilidad',
      GEOGRAPHIC: 'Restricción geográfica',
      LIMITED_STOCK: 'Stock limitado',
    };
    return labels[type] ?? type;
  }
}
