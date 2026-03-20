import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../../../core/services/language.service';

@Component({
  selector: 'app-hero',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hero.component.html',
  styleUrl: './hero.component.scss'
})
export class HeroComponent implements OnInit, OnDestroy {
  currentTitle = '';
  private titleIndex = 0;
  private charIndex = 0;
  private isDeleting = false;
  private timer: any;

  constructor(public langService: LanguageService) {}

  ngOnInit(): void {
    this.typeWriter();
  
    // 🔥 performans için ekliyoruz
    setTimeout(() => {
      document.getElementById('hero')?.classList.add('loaded');
    }, 600);
  }

  ngOnDestroy(): void {
    clearTimeout(this.timer);
  }

  get titles() {
    return this.langService.t.hero.titles;
  }

  private typeWriter(): void {
    const current = this.titles[this.titleIndex % this.titles.length];

    if (this.isDeleting) {
      this.currentTitle = current.substring(0, this.charIndex - 1);
      this.charIndex--;
    } else {
      this.currentTitle = current.substring(0, this.charIndex + 1);
      this.charIndex++;
    }

    let speed = this.isDeleting ? 60 : 100;

    if (!this.isDeleting && this.charIndex === current.length) {
      speed = 2000;
      this.isDeleting = true;
    } else if (this.isDeleting && this.charIndex === 0) {
      this.isDeleting = false;
      this.titleIndex = (this.titleIndex + 1) % this.titles.length;
      speed = 400;
    }

    this.timer = setTimeout(() => this.typeWriter(), speed);
  }

  scrollTo(id: string): void {
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' });
  }

  terminalLines = [
    { cmd: 'dotnet new webapi -n Portfolio', output: '' },
    { cmd: 'git commit -m "feat: add JWT auth"', output: '✓ committed' },
    { cmd: 'ng build --configuration production', output: '✓ build success' },
    { cmd: 'docker run -p 5432:5432 postgres', output: '✓ running' },
  ];
}