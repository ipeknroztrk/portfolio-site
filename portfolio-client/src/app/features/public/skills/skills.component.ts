import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { Skill } from '../../../core/models/skill.model';
import { LanguageService } from '../../../core/services/language.service';
import { FadeUpDirective } from '../../../core/directives/fade-up.directive';

@Component({
  selector: 'app-skills',
  standalone: true,
  imports: [CommonModule, FadeUpDirective],
  templateUrl: './skills.component.html',
  styleUrl: './skills.component.scss'
})
export class SkillsComponent implements OnInit {
  skills: Skill[] = [];
  categories: string[] = [];
  isLoading = true;

  staticSkills: Skill[] = [
    { name: 'C#', category: 'Backend', level: 5, orderIndex: 1, isVisible: true },
    { name: 'ASP.NET Core', category: 'Backend', level: 5, orderIndex: 2, isVisible: true },
    { name: 'Entity Framework Core', category: 'Backend', level: 4, orderIndex: 3, isVisible: true },
    { name: 'RESTful API', category: 'Backend', level: 5, orderIndex: 4, isVisible: true },
    { name: 'PostgreSQL', category: 'Database', level: 4, orderIndex: 1, isVisible: true },
    { name: 'SQL Server', category: 'Database', level: 4, orderIndex: 2, isVisible: true },
    { name: 'Angular', category: 'Frontend', level: 4, orderIndex: 1, isVisible: true },
    { name: 'TypeScript', category: 'Frontend', level: 4, orderIndex: 2, isVisible: true },
    { name: 'React', category: 'Frontend', level: 3, orderIndex: 3, isVisible: true },
    { name: 'HTML & CSS', category: 'Frontend', level: 4, orderIndex: 4, isVisible: true },
    { name: 'Swift', category: 'Other', level: 3, orderIndex: 1, isVisible: true },
    { name: 'Python', category: 'Other', level: 3, orderIndex: 2, isVisible: true },
    { name: 'Git & GitHub', category: 'Other', level: 5, orderIndex: 3, isVisible: true },
  ];

  constructor(
    private portfolioService: PortfolioService,
    public langService: LanguageService
  ) {}

  ngOnInit(): void {
    this.portfolioService.getSkills().subscribe({
      next: (data) => {
        const source = data.length > 0 ? data : this.staticSkills;
        this.skills = source;
        this.categories = [...new Set(source.map(s => s.category))];
        this.isLoading = false;
      },
      error: () => {
        this.skills = this.staticSkills;
        this.categories = [...new Set(this.staticSkills.map(s => s.category))];
        this.isLoading = false;
      }
    });
  }

  getSkillsByCategory(cat: string): Skill[] {
    return this.skills.filter(s => s.category === cat);
  }

  getLevelPercent(level: number): number {
    return (level / 5) * 100;
  }

  getLevelLabel(level: number): string {
    const levels = this.langService.t.skills.levels as any;
    return levels[level] || '';
  }
  getDots(level: number): number[] {
    return Array(level).fill(0);
  }
  
  getEmptyDots(level: number): number[] {
    return Array(5 - level).fill(0);
  }
}