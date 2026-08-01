import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private state = signal<{ message: string; resolve?: (v: boolean) => void } | null>(null);

  readonly isOpen = signal(false);
  readonly message = signal('');

  confirm(message: string): Promise<boolean> {
    return new Promise(resolve => {
      this.message.set(message);
      this.isOpen.set(true);
      this.state.set({ message, resolve });
    });
  }

  accept(): void {
    this.state()?.resolve?.(true);
    this.close();
  }

  cancel(): void {
    this.state()?.resolve?.(false);
    this.close();
  }

  private close(): void {
    this.isOpen.set(false);
    this.state.set(null);
  }
}
