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
    this.menuOpen = false;
  
    // Oyun butonuna tıklanınca yıldız efekti
    if (sectionId === 'game') {
      this.spawnStars();
    }
  
    setTimeout(() => {
      const el = document.getElementById(sectionId);
      if (!el) return;
      const y = el.getBoundingClientRect().top + window.scrollY - 80;
      window.scrollTo({ top: y, behavior: 'smooth' });
    }, sectionId === 'game' ? 600 : 10);
  }
  
  private starInterval: any;

  private spawnStars(): void {
    const colors = ['#fbbf24', '#06b6d4', '#c0c0c0', '#fff', '#fbbf24'];
  
    // CSS animasyonu ekle
    if (!document.getElementById('star-fall-style')) {
      const style = document.createElement('style');
      style.id = 'star-fall-style';
      style.textContent = `
        @keyframes starFall {
          0% { transform: translateY(-20px) rotate(0deg) scale(1); opacity: 1; }
          100% { transform: translateY(110vh) rotate(720deg) scale(0.3); opacity: 0; }
        }
      `;
      document.head.appendChild(style);
    }
  
    // Sürekli yıldız spawn et
    clearInterval(this.starInterval);
    this.starInterval = setInterval(() => {
      for (let i = 0; i < 3; i++) {
        const star = document.createElement('div');
        const symbols = ['✦', '★', '✸', '✺', '⭐'];
        star.innerHTML = symbols[Math.floor(Math.random() * symbols.length)];
        const duration = 0.8 + Math.random() * 1.2;
        star.style.cssText = `
          position: fixed;
          z-index: 999999;
          pointer-events: none;
          font-size: ${10 + Math.random() * 24}px;
          color: ${colors[Math.floor(Math.random() * colors.length)]};
          left: ${Math.random() * 100}vw;
          top: -30px;
          animation: starFall ${duration}s ease-in forwards;
          text-shadow: 0 0 8px currentColor;
        `;
        document.body.appendChild(star);
        setTimeout(() => star.remove(), duration * 1000 + 100);
      }
    }, 80);
  
    // Game section'a ulaşınca durdur
    setTimeout(() => {
      const el = document.getElementById('game');
      if (!el) return;
  
      const checkArrival = setInterval(() => {
        const rect = el.getBoundingClientRect();
        if (rect.top <= 150) {
          clearInterval(this.starInterval);
          clearInterval(checkArrival);
        }
      }, 100);
    }, 200);
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
