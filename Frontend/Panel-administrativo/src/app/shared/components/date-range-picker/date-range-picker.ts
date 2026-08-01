import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-date-range-picker',
  imports: [],
  templateUrl: './date-range-picker.html',
  styleUrl: './date-range-picker.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DateRangePickerComponent {
  readonly startDate = input('');
  readonly endDate = input('');
  readonly dateChange = output<{ start: string; end: string }>();

  protected onStartChange(value: string): void {
    this.dateChange.emit({ start: value, end: this.endDate() });
  }

  protected onEndChange(value: string): void {
    this.dateChange.emit({ start: this.startDate(), end: value });
  }
}
