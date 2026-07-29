import { Component, input, output } from '@angular/core';

export type ButtonVariant = 'primary' | 'secondary' | 'danger';
export type ButtonSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-btn',
  imports: [],
  templateUrl: './button.html',
  styleUrl: './button.scss',
})
export class ButtonComponent {
  readonly variant = input<ButtonVariant>('primary');
  readonly size = input<ButtonSize>('md');
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly type = input<'button' | 'submit'>('button');
  readonly onClick = output<void>();
}
