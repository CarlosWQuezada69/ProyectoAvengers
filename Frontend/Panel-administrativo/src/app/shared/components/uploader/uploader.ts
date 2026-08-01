import { Component, input, output, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-uploader',
  imports: [],
  templateUrl: './uploader.html',
  styleUrl: './uploader.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UploaderComponent {
  readonly accept = input('image/*');
  readonly multiple = input(false);
  readonly maxSizeMb = input(5);
  readonly filesChange = output<File[]>();

  protected dragging = false;
  protected error = '';

  protected onDragOver(e: DragEvent): void {
    e.preventDefault();
    this.dragging = true;
  }

  protected onDragLeave(): void {
    this.dragging = false;
  }

  protected onDrop(e: DragEvent): void {
    e.preventDefault();
    this.dragging = false;
    const files = Array.from(e.dataTransfer?.files ?? []);
    this.processFiles(files);
  }

  protected onFileSelect(e: Event): void {
    const files = Array.from((e.target as HTMLInputElement).files ?? []);
    this.processFiles(files);
  }

  private processFiles(files: File[]): void {
    this.error = '';
    const valid = files.filter(f => {
      if (f.size > this.maxSizeMb() * 1024 * 1024) {
        this.error = `Archivo ${f.name} excede el límite de ${this.maxSizeMb()}MB`;
        return false;
      }
      return true;
    });
    if (valid.length) {
      this.filesChange.emit(valid);
    }
  }
}
