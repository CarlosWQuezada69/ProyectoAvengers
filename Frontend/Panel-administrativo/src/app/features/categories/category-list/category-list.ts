import { Component, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoriesService } from '../../../core/services/categories.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { InputComponent } from '../../../shared/components/input/input';
import { ModalComponent } from '../../../shared/components/modal/modal';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { HasPermissionDirective } from '../../../shared/directives/has-permission.directive';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import { ToastService } from '../../../shared/services/toast.service';
import type { Category } from '../../../core/models/category';

@Component({
  selector: 'app-category-list',
  imports: [ReactiveFormsModule, ButtonComponent, InputComponent, ModalComponent, BadgeComponent, HasPermissionDirective],
  templateUrl: './category-list.html',
  styleUrl: './category-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategoryListComponent implements OnInit {
  private categoriesService = inject(CategoriesService);
  private toast = inject(ToastService);
  private confirmDialog = inject(ConfirmDialogService);

  protected categories: Category[] = [];
  protected loading = true;
  protected modalOpen = false;
  protected editingCategory: Category | null = null;

  protected form = new FormGroup({
    name: new FormControl('', [Validators.required]),
    slug: new FormControl('', [Validators.required]),
    description: new FormControl(''),
    parentCategoryId: new FormControl(''),
    isActive: new FormControl(true),
    displayOrder: new FormControl(0),
  });

  ngOnInit(): void {
    this.loadCategories();
  }

  private loadCategories(): void {
    this.categoriesService.list(true).subscribe({
      next: res => {
        this.categories = Array.isArray(res) ? res : (res as any).data ?? [];
        this.loading = false;
      },
      error: () => this.loading = false,
    });
  }

  protected openCreateModal(): void {
    this.editingCategory = null;
    this.form.reset({ isActive: true, displayOrder: 0 });
    this.modalOpen = true;
  }

  protected openEditModal(cat: Category): void {
    this.editingCategory = cat;
    this.form.patchValue(cat);
    this.modalOpen = true;
  }

  protected save(): void {
    if (this.form.invalid) return;
    const data = this.form.value as any;
    if (data.parentCategoryId === '') data.parentCategoryId = null;

    if (this.editingCategory) {
      this.categoriesService.update(this.editingCategory.id, data).subscribe({
        next: () => {
          this.toast.show('Categoría actualizada', 'success');
          this.modalOpen = false;
          this.loadCategories();
        },
      });
    } else {
      this.categoriesService.create(data).subscribe({
        next: () => {
          this.toast.show('Categoría creada', 'success');
          this.modalOpen = false;
          this.loadCategories();
        },
      });
    }
  }

  protected async deleteCategory(id: string): Promise<void> {
    const confirmed = await this.confirmDialog.confirm('¿Eliminar esta categoría?');
    if (!confirmed) return;
    this.categoriesService.delete(id).subscribe({
      next: () => {
        this.toast.show('Categoría eliminada', 'success');
        this.loadCategories();
      },
      error: () => {
        this.toast.show('No se puede eliminar: tiene productos asociados', 'error');
      },
    });
  }

  protected flatCategories(): Category[] {
    const result: Category[] = [];
    const flatten = (items: Category[], depth = 0) => {
      for (const item of items) {
        result.push({ ...item, name: '—'.repeat(depth) + ' ' + item.name });
        if (item.children) flatten(item.children, depth + 1);
      }
    };
    flatten(this.categories);
    return result;
  }
}
