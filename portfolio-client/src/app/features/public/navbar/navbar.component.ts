import { Component, HostListener, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  activeSection = 'hero';
  isScrolled = false;
  menuOpen = false;

  constructor(
    public langService: LanguageService,
    private ngZone: NgZone
  ) {}

  get navItems() {
    const t = this.langService.t.nav;
    return [
      { label: t.about, target: 'about' },
      { label: t.experience, target: 'experience' },
      { label: t.projects, target: 'projects' },
      { label: t.skills, target: 'skills' },
      { label: t.contact, target: 'contact' },
    ];
  }

  @HostListener('window:scroll')
  onScroll(): void {
    this.isScrolled = window.scrollY > 50;
    this.updateActiveSection();
  }
  scrollTo(sectionId: string): void {
    console.log('scrollTo çağrıldı:', sectionId);
    this.menuOpen = false;
  
    setTimeout(() => {
      const el = document.getElementById(sectionId);
      console.log('element:', el);
      if (!el) return;
  
      const y = el.getBoundingClientRect().top + window.scrollY - 80;
      console.log('y:', y, 'window.scrollY:', window.scrollY);
      window.scrollTo({ top: y, behavior: 'smooth' });
    }, 10);
  }
  
  private updateActiveSection(): void {
    const sections = ['hero', 'about', 'experience', 'projects', 'skills', 'contact', 'game'];
    for (const id of sections) {
      const el = document.getElementById(id);
      if (el) {
        const rect = el.getBoundingClientRect();
        if (rect.top <= 100 && rect.bottom >= 100) {
          this.ngZone.run(() => { this.activeSection = id; });
          break;
        }
      }
    }
  }
}
