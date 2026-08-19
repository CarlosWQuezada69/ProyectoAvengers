import { Component, inject, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { SliderService } from '../../../core/services/slider.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { InputComponent } from '../../../shared/components/input/input';
import { ModalComponent } from '../../../shared/components/modal/modal';
import { BadgeComponent } from '../../../shared/components/badge/badge';
import { UploaderComponent } from '../../../shared/components/uploader/uploader';
import { HasPermissionDirective } from '../../../shared/directives/has-permission.directive';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import { ToastService } from '../../../shared/services/toast.service';
import type { SliderItem } from '../../../core/models/index';

@Component({
  selector: 'app-slider-list',
  imports: [DatePipe, ReactiveFormsModule, ButtonComponent, InputComponent, ModalComponent, BadgeComponent, UploaderComponent, HasPermissionDirective],
  templateUrl: './slider-list.html',
  styleUrl: './slider-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SliderListComponent implements OnInit {
  private sliderService = inject(SliderService);
  private toast = inject(ToastService);
  private confirmDialog = inject(ConfirmDialogService);

  protected items = signal<SliderItem[]>([]);
  protected loading = true;
  protected modalOpen = false;
  protected editingItem: SliderItem | null = null;

  protected form = new FormGroup({
    title: new FormControl(''),
    subtitle: new FormControl(''),
    linkUrl: new FormControl(''),
    startsAt: new FormControl(''),
    endsAt: new FormControl(''),
    isActive: new FormControl(true),
  });

  ngOnInit(): void {
    this.loadItems();
  }

  private loadItems(): void {
    this.sliderService.list().subscribe({
      next: data => { this.items.set(data); this.loading = false;
        if (data.length === 0) this.toast.info('Aún no hay ítems en el slider');
      },
      error: () => this.loading = false,
    });
  }

  protected openCreateModal(): void {
    this.editingItem = null;
    this.form.reset({ isActive: true });
    this.modalOpen = true;
  }

  protected openEditModal(item: SliderItem): void {
    this.editingItem = item;
    this.form.patchValue({
      ...item,
      startsAt: item.startsAt?.split('T')[0] ?? '',
      endsAt: item.endsAt?.split('T')[0] ?? '',
    });
    this.modalOpen = true;
  }

  protected save(): void {
    if (this.editingItem) {
      this.sliderService.update(this.editingItem.id, this.form.value as any).subscribe({
        next: () => { this.toast.show('Slider actualizado', 'success'); this.modalOpen = false; this.loadItems(); },
      });
    } else {
      const fd = new FormData();
      Object.entries(this.form.value).forEach(([k, v]) => fd.append(k, v as string));
      this.sliderService.create(fd).subscribe({
        next: () => { this.toast.show('Slider creado', 'success'); this.modalOpen = false; this.loadItems(); },
      });
    }
  }

  protected async deleteItem(id: string): Promise<void> {
    const confirmed = await this.confirmDialog.confirm('¿Eliminar este ítem del slider?');
    if (!confirmed) return;
    this.sliderService.delete(id).subscribe(() => {
      this.toast.show('Slider eliminado', 'success');
      this.loadItems();
    });
  }
}
