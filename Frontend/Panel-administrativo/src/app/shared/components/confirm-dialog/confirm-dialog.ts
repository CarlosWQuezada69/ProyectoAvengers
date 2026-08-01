import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { ModalComponent } from '../modal/modal';
import { ButtonComponent } from '../button/button';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';

@Component({
  selector: 'app-confirm-dialog',
  imports: [ModalComponent, ButtonComponent],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialogComponent {
  protected readonly service = inject(ConfirmDialogService);
}
