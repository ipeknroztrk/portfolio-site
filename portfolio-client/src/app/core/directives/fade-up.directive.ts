import { Directive, ElementRef, OnInit, OnDestroy } from '@angular/core';

@Directive({
  selector: '[fadeUp]',
  standalone: true
})
export class FadeUpDirective implements OnInit, OnDestroy {
  private observer!: IntersectionObserver;

  constructor(private el: ElementRef) {}

  ngOnInit(): void {
    // Başlangıçta görünmez yap
    this.el.nativeElement.style.opacity = '0';
    this.el.nativeElement.style.transform = 'translateY(30px)';
    this.el.nativeElement.style.transition = 'opacity 0.7s ease, transform 0.7s ease';

    // Ekrana girince animasyonu tetikle
    this.observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          this.el.nativeElement.style.opacity = '1';
          this.el.nativeElement.style.transform = 'translateY(0)';
          this.observer.unobserve(this.el.nativeElement);
        }
      },
      { threshold: 0.15 }
    );

    this.observer.observe(this.el.nativeElement);
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }
}