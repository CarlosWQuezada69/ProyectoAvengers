import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private toastsSignal = signal<Toast[]>([]);
  private nextId = 0;

  readonly toasts = this.toastsSignal.asReadonly();

  show(message: string, type: Toast['type'] = 'info', duration = 4000): void {
    const id = this.nextId++;
    this.toastsSignal.update(t => [...t, { id, message, type }]);
    setTimeout(() => this.dismiss(id), duration);
  }

  dismiss(id: number): void {
    this.toastsSignal.update(t => t.filter(toast => toast.id !== id));
  }
}
