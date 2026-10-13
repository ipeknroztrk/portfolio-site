import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { retry, timer } from 'rxjs';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { Experience } from '../../../core/models/experience.model';
import { LanguageService } from '../../../core/services/language.service';
import { FadeUpDirective } from '../../../core/directives/fade-up.directive';

@Component({
  selector: 'app-experience',
  standalone: true,
  imports: [CommonModule, FadeUpDirective],
  templateUrl: './experience.component.html',
  styleUrl: './experience.component.scss'
})
export class ExperienceComponent implements OnInit {
  experiences: Experience[] = [];
  isLoading = true;
  hasError = false;

  trainings = [
    { name: 'BilgeAdam Academy', detail: 'Full Stack — Angular, .NET, SQL', year: '2025' },
    { name: 'Acunmedya Academy', detail: 'C# .NET — ASP.NET Core MVC, EF Core', year: '2024' },
    { name: 'Global AI Hub', detail: 'Python Bootcamp', year: '2024' },
    { name: 'Turkcell Gelecegi Yazanlar', detail: 'C# & Python 101/201', year: '2023' },
    { name: 'BTK Akademi', detail: 'API ve API Test', year: '2023' },
  ];

  constructor(
    private portfolioService: PortfolioService,
    public langService: LanguageService
  ) {}

  ngOnInit(): void {
    // Soğuk başlangıştaki API'yi (Render vb.) beklemek için artan aralıklarla tekrar dener.
    this.portfolioService.getExperiences().pipe(
      retry({ count: 6, delay: (_error, retryCount) => timer(retryCount * 3000) })
    ).subscribe({
      next: (data) => { this.experiences = data; this.isLoading = false; this.hasError = false; },
      error: (err) => {
        console.error('Deneyimler yüklenemedi:', err);
        this.isLoading = false;
        this.hasError = true;
      }
    });
  }

  formatDate(date: Date | undefined): string {
    if (!date) return this.langService.t.experience.present;
    return new Date(date).toLocaleDateString(
      this.langService.current() === 'tr' ? 'tr-TR' : 'en-US',
      { year: 'numeric', month: 'short' }
    );
  }
}