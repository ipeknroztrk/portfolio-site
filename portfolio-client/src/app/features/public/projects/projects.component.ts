import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { Project } from '../../../core/models/project.model';
import { LanguageService } from '../../../core/services/language.service';
import { FadeUpDirective } from '../../../core/directives/fade-up.directive';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, FadeUpDirective],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss'
})
export class ProjectsComponent implements OnInit {
  projects: Project[] = [];
  isLoading = true;
  selectedProject: Project | null = null;

  staticProjects: Project[] = [
    {
      title: 'Eventura',
      description: 'Etkinlik ve bilet satış platformu. Katmanlı mimari (Controller, Service, Repository), kimlik doğrulama ve rol tabanlı admin panel içeriyor.',
      techStack: 'ASP.NET Core MVC, EF Core, PostgreSQL',
      githubUrl: 'https://github.com/ipeknroztrk/Eventura',
      isVisible: true, orderIndex: 1
    },
    {
      title: 'VakıfBank POS Inventory',
      description: 'Bankacılık operasyonları için POS envanter ve yönetim sistemi.',
      techStack: '.NET Core MVC, PostgreSQL',
      isVisible: true, orderIndex: 2
    },
    {
      title: 'Quizzy',
      description: 'iOS quiz uygulaması. Firebase Authentication ve Firestore ile gerçek zamanlı veri yönetimi.',
      techStack: 'Swift, Firebase, Firestore',
      isVisible: true, orderIndex: 3
    },
    {
      title: 'Portfolio Site',
      description: 'Bu site! Angular 18 + .NET 8 + PostgreSQL ile geliştirildi. JWT auth, admin panel, EF Core Code First.',
      techStack: 'Angular 18, ASP.NET Core, EF Core, PostgreSQL',
      githubUrl: 'https://github.com/ipeknroztrk',
      isVisible: true, orderIndex: 4
    }
  ];

  constructor(
    private portfolioService: PortfolioService,
    public langService: LanguageService
  ) {}

  ngOnInit(): void {
    this.portfolioService.getPublicProjects().subscribe({
      next: (data) => {
        this.projects = data.length > 0 ? data : this.staticProjects;
        this.isLoading = false;
      },
      error: () => {
        this.projects = this.staticProjects;
        this.isLoading = false;
      }
    });
  }

  getTechList(techStack: string): string[] {
    return techStack.split(',').map(t => t.trim());
  }

  onMouseMove(event: MouseEvent, card: HTMLElement): void {
    const rect = card.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;
    const centerX = rect.width / 2;
    const centerY = rect.height / 2;
    const rotateX = ((y - centerY) / centerY) * -8;
    const rotateY = ((x - centerX) / centerX) * 8;
    card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-6px)`;
  }
  
  onMouseLeave(card: HTMLElement): void {
    card.style.transform = 'perspective(1000px) rotateX(0) rotateY(0) translateY(0)';
  }

  openModal(project: Project): void { this.selectedProject = project; }
  closeModal(): void { this.selectedProject = null; }
}