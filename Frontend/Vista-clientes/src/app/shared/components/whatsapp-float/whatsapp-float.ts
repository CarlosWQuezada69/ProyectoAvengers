import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';

import { CatalogService } from '../../../core/services/catalog.service';
import {
  buildWhatsAppLink,
  DEFAULT_WHATSAPP_MESSAGE,
  resolveWhatsAppNumber
} from '../../../core/utils/whatsapp';
import { IconComponent } from '../../icon/icon';

@Component({
  selector: 'app-whatsapp-float',
  standalone: true,
  imports: [IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './whatsapp-float.html',
  styleUrls: ['./whatsapp-float.scss']
})
export class WhatsAppFloatComponent {
  private readonly catalog = inject(CatalogService);

  readonly href = computed(() => {
    const phone = resolveWhatsAppNumber(this.catalog.settings());
    return phone ? buildWhatsAppLink(phone, DEFAULT_WHATSAPP_MESSAGE) : null;
  });
}