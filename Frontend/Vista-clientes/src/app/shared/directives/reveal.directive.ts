import { Directive, ElementRef, NgZone, OnDestroy, OnInit, inject, input, signal } from '@angular/core';

export type RevealVariant = 'up' | 'left' | 'right' | 'fade' | 'scale';

@Directive({
  selector: '[reveal]',
  standalone: true,
  host: {
    '[class.is-in-view]': 'inView()',
    '[class.anim-reveal]': "variant() === 'up'",
    '[class.anim-reveal-left]': "variant() === 'left'",
    '[class.anim-reveal-right]': "variant() === 'right'",
    '[class.anim-fade]': "variant() === 'fade'",
    '[class.anim-scale]': "variant() === 'scale'"
  }
})
export class RevealDirective implements OnInit, OnDestroy {
  readonly variant = input<RevealVariant>('up');
  readonly revealDelay = input(0);

  private readonly el = inject(ElementRef<HTMLElement>);
  private readonly zone = inject(NgZone);
  readonly inView = signal(false);
  private observer: IntersectionObserver | null = null;

  ngOnInit(): void {
    if (this.revealDelay() > 0) {
      this.el.nativeElement.style.setProperty('--reveal-delay', `${this.revealDelay()}ms`);
    }

    this.zone.runOutsideAngular(() => {
      if (typeof IntersectionObserver === 'undefined') {
        this.inView.set(true);
        return;
      }

      this.observer = new IntersectionObserver(
        (entries) => {
          if (entries[0]?.isIntersecting) {
            this.zone.run(() => this.inView.set(true));
            this.observer?.disconnect();
          }
        },
        { threshold: 0.12, rootMargin: '0px 0px -8% 0px' }
      );
      this.observer.observe(this.el.nativeElement);
    });
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}