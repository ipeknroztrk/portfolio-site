import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../../../core/services/language.service';
import { FadeUpDirective } from '../../../core/directives/fade-up.directive';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, FadeUpDirective],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss'
})
export class AboutComponent {
  techs = [
    'C#', 'ASP.NET Core', 'Entity Framework Core',
    'PostgreSQL', 'SQL Server', 'RESTful API',
    'Angular', 'TypeScript', 'React',
    'Git', 'GitHub', 'Swift', 'Python'
  ];

  constructor(public langService: LanguageService) {}
}