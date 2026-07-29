import { Component, inject } from '@angular/core';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast-container',
  imports: [],
  templateUrl: './toast.html',
  styleUrl: './toast.scss',
})
export class ToastContainerComponent {
  protected toastService = inject(ToastService);
  readonly toasts = this.toastService.toasts;
}
