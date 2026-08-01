import { Component, inject, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { SettingsService } from '../../../core/services/settings.service';
import { BrandingService } from '../../../core/services/branding.service';
import { environment } from '../../../../environments/environment';
import { ButtonComponent } from '../../../shared/components/button/button';
import { InputComponent } from '../../../shared/components/input/input';
import { UploaderComponent } from '../../../shared/components/uploader/uploader';
import { ToastService } from '../../../shared/services/toast.service';
import type { SiteSetting } from '../../../core/models/index';

type SettingKeys = 'business_name' | 'copyright_text' | 'contact_email' | 'contact_phone' | 'social_links';

@Component({
  selector: 'app-settings-form',
  imports: [ReactiveFormsModule, ButtonComponent, InputComponent, UploaderComponent],
  templateUrl: './settings-form.html',
  styleUrl: './settings-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsFormComponent implements OnInit {
  private settingsService = inject(SettingsService);
  private branding = inject(BrandingService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  protected settings: SiteSetting[] = [];
  protected loading = true;
  protected logoUrl = '';

  protected form = new FormGroup({
    business_name: new FormControl(''),
    copyright_text: new FormControl(''),
    contact_email: new FormControl(''),
    contact_phone: new FormControl(''),
    social_links: new FormControl(''),
  });

  ngOnInit(): void {
    this.loadSettings();
    setTimeout(() => {
      if (this.loading) {
        this.loading = false;
        this.cdr.detectChanges();
      }
    }, 5000);
  }

  private loadSettings(): void {
    this.settingsService.getAll().subscribe({
      next: data => {
        this.settings = data;
        for (const s of data) {
          if (s.key === 'logo_url') { this.logoUrl = s.value ?? ''; }
          if (s.key in this.form.controls) {
            this.form.controls[s.key as SettingKeys].setValue(s.value ?? '');
          }
        }
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.loading = false;
        this.cdr.detectChanges();
      },
    });
  }

  protected save(key: string): void {
    const value = this.form.get(key as SettingKeys)?.value ?? '';
    this.settingsService.update(key, value).subscribe(() => {
      this.branding.refresh();
      this.toast.show('Configuración guardada', 'success');
    });
  }

  protected onLogoUpload(files: File[]): void {
    if (!files[0]) return;
    this.settingsService.uploadLogo(files[0]).subscribe({
      next: res => {
        this.logoUrl = this.toAbsolute(res.value ?? '');
        this.branding.refresh();
        this.cdr.markForCheck();
        this.toast.show('Logo actualizado', 'success');
      },
      error: () => this.toast.show('Error al subir el logo', 'error'),
    });
  }

  private toAbsolute(url: string): string {
    if (!url || /^https?:\/\//.test(url)) return url;
    return `${environment.apiUrl.replace('/api/v1', '')}${url}`;
  }
}
