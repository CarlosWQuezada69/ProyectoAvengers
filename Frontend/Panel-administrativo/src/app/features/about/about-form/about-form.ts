import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';

import { environment } from '../../../../environments/environment';
import { AboutService } from '../../../core/services/about.service';
import { ButtonComponent } from '../../../shared/components/button/button';
import { InputComponent } from '../../../shared/components/input/input';
import { UploaderComponent } from '../../../shared/components/uploader/uploader';
import { ToastService } from '../../../shared/services/toast.service';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';
import type { AboutGallery } from '../../../core/models/about';

@Component({
  selector: 'app-about-form',
  imports: [ReactiveFormsModule, ButtonComponent, InputComponent, UploaderComponent],
  templateUrl: './about-form.html',
  styleUrl: './about-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AboutFormComponent implements OnInit {
  private aboutService = inject(AboutService);
  private toast = inject(ToastService);
  private confirmDialog = inject(ConfirmDialogService);
  private cdr = inject(ChangeDetectorRef);

  protected loading = true;
  protected saving = false;
  protected gallery = signal<AboutGallery[]>([]);
  protected gallerySection = 'default';

  protected form = new FormGroup({
    title: new FormControl(''),
    history: new FormControl(''),
    mission: new FormControl(''),
    vision: new FormControl(''),
  });

  ngOnInit(): void {
    this.load();
    setTimeout(() => {
      if (this.loading) {
        this.loading = false;
        this.cdr.detectChanges();
      }
    }, 5000);
  }

  private load(): void {
    this.aboutService.getAbout().subscribe({
      next: about => {
        this.form.patchValue({
          title: about.title ?? '',
          history: about.history ?? '',
          mission: about.mission ?? '',
          vision: about.vision ?? '',
        });
        this.gallery.set(about.gallery ?? []);
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  protected save(): void {
    const raw = this.form.getRawValue();
    this.saving = true;
    this.aboutService
      .updateAbout({
        title: raw.title ?? '',
        history: raw.history ?? '',
        mission: raw.mission?.trim() ? raw.mission : null,
        vision: raw.vision?.trim() ? raw.vision : null,
      })
      .subscribe({
        next: () => {
          this.saving = false;
          this.cdr.detectChanges();
          this.toast.show('Información guardada', 'success');
        },
        error: () => {
          this.saving = false;
          this.cdr.detectChanges();
        },
      });
  }

  protected setGallerySection(value: string): void {
    this.gallerySection = value;
  }

  protected onUpload(files: File[]): void {
    const file = files[0];
    if (!file) return;
    this.aboutService.uploadGallery(file, this.gallerySection).subscribe({
      next: image => {
        this.gallery.update(items => [...items, image]);
        this.cdr.markForCheck();
        this.toast.show('Imagen subida', 'success');
      },
      error: () => this.toast.show('Error al subir la imagen', 'error'),
    });
  }

  protected async deleteImage(id: string): Promise<void> {
    const confirmed = await this.confirmDialog.confirm('¿Eliminar esta imagen de la galería?');
    if (!confirmed) return;
    this.aboutService.deleteGallery(id).subscribe({
      next: () => {
        this.gallery.update(items => items.filter(i => i.id !== id));
        this.cdr.markForCheck();
        this.toast.show('Imagen eliminada', 'success');
      },
    });
  }

  protected move(index: number, direction: -1 | 1): void {
    const items = [...this.gallery()];
    const target = index + direction;
    if (target < 0 || target >= items.length) return;

    const current = items[index];
    items[index] = items[target];
    items[target] = current;
    this.gallery.set(items);
    this.persistOrder();
  }

  private persistOrder(): void {
    const order = this.gallery().map((item, index) => ({ id: item.id, displayOrder: index }));
    this.aboutService.reorderGallery(order).subscribe({
      error: () => this.toast.show('Error al guardar el orden', 'error'),
    });
  }

  protected toAbsolute(url: string): string {
    if (!url || /^https?:\/\//.test(url)) return url;
    return `${environment.apiUrl.replace('/api/v1', '')}${url}`;
  }
}