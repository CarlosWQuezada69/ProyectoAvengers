import { Component, inject, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { SettingsService } from '../../../core/services/settings.service';
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
})
export class SettingsFormComponent implements OnInit {
  private settingsService = inject(SettingsService);
  private toast = inject(ToastService);

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
      },
      error: () => this.loading = false,
    });
  }

  protected save(key: string): void {
    const value = this.form.get(key as SettingKeys)?.value ?? '';
    this.settingsService.update(key, value).subscribe(() => {
      this.toast.show('Configuración guardada', 'success');
    });
  }

  protected onLogoUpload(files: File[]): void {
    this.settingsService.uploadLogo(files[0]).subscribe(res => {
      this.logoUrl = res.url;
      this.toast.show('Logo actualizado', 'success');
    });
  }
}
