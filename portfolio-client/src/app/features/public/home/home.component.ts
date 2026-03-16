import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../navbar/navbar.component';
import { HeroComponent } from '../hero/hero.component';
import { AboutComponent } from '../about/about.component';
import { ExperienceComponent } from '../experience/experience.component';
import { ProjectsComponent } from '../projects/projects.component';
import { SkillsComponent } from '../skills/skills.component';
import { ContactComponent } from '../contact/contact.component';
import { StarsBgComponent } from '../stars-bg/stars-bg.component';
import { CursorComponent } from '../cursor/cursor.component';
import { GameComponent } from '../game/game.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    StarsBgComponent,
    CursorComponent,
    NavbarComponent,
    HeroComponent,
    AboutComponent,
    ExperienceComponent,
    ProjectsComponent,
    SkillsComponent,
    ContactComponent,
    GameComponent
  ],
  template: `
    <app-stars-bg />
    <app-cursor />
    <div class="scroll-progress" [style.width.%]="scrollPercent"></div>
    <app-navbar />
    <main>
      <app-hero />
      <app-about />
      <app-experience />
      <app-projects />
      <app-skills />
      <app-contact />
      <app-game />
    </main>
  `,
  styles: [`
    .scroll-progress {
      position: fixed;
      top: 0; left: 0;
      height: 2px;
      background: linear-gradient(90deg, #06b6d4, #c0c0c0, #06b6d4);
      z-index: 99999;
      transition: width 0.1s linear;
      box-shadow: 0 0 8px #06b6d4;
    }
  `]
})
export class HomeComponent implements OnInit {
  scrollPercent = 0;

  ngOnInit(): void {
    // Sayfa yüklenince scroll'u en üste al
    window.scrollTo(0, 0);
    document.documentElement.style.overflow = 'auto';
    document.body.style.overflow = 'auto';
    document.body.style.height = 'auto';
  }

  @HostListener('window:scroll')
  onScroll(): void {
    const scrollTop = window.scrollY;
    const docHeight = document.documentElement.scrollHeight - window.innerHeight;
    this.scrollPercent = docHeight > 0 ? (scrollTop / docHeight) * 100 : 0;
  }
}