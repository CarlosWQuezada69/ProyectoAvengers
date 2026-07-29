import { Directive, inject, input, TemplateRef, ViewContainerRef, effect } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Directive({
  selector: '[hasPermission]',
  standalone: true,
})
export class HasPermissionDirective {
  readonly hasPermission = input<string>();

  private authService = inject(AuthService);
  private templateRef = inject(TemplateRef<unknown>);
  private viewContainer = inject(ViewContainerRef);

  constructor() {
    effect(() => {
      const permission = this.hasPermission();
      if (!permission || this.authService.hasPermission(permission)) {
        this.viewContainer.createEmbeddedView(this.templateRef);
      } else {
        this.viewContainer.clear();
      }
    });
  }
}
