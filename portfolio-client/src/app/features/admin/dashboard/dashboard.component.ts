import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { Project } from '../../../core/models/project.model';
import { Experience } from '../../../core/models/experience.model';
import { Skill } from '../../../core/models/skill.model';
import { ContactMessage } from '../../../core/models/contact.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  activeTab: 'projects' | 'experiences' | 'skills' | 'messages' = 'projects';
  projects: Project[] = [];
  experiences: Experience[] = [];
  skills: Skill[] = [];
  messages: ContactMessage[] = [];
  projectForm: Project = this.emptyProject();
  experienceForm: Experience = this.emptyExperience();
  skillForm: Skill = this.emptySkill();
  editingProjectId: number | null = null;
  editingExperienceId: number | null = null;
  editingSkillId: number | null = null;

  constructor(
    private authService: AuthService,
    private portfolioService: PortfolioService,
    private router: Router
  ) {}

  ngOnInit(): void { this.loadAll(); }

  loadAll(): void {
    this.portfolioService.getProjects().subscribe(d => this.projects = d);
    this.portfolioService.getExperiences().subscribe(d => this.experiences = d);
    this.portfolioService.getSkills().subscribe(d => this.skills = d);
    this.portfolioService.getMessages().subscribe(d => this.messages = d);
  }

  saveProject(): void {
    if (this.editingProjectId) {
      this.portfolioService.updateProject(this.editingProjectId, this.projectForm)
        .subscribe({
          next: (updated) => {
            this.projects = this.projects.map(p => p.id === updated.id ? updated : p);
            this.resetProjectForm();
          }
        });
    } else {
      this.portfolioService.createProject(this.projectForm)
        .subscribe({
          next: (created) => {
            this.projects = [...this.projects, created];
            this.resetProjectForm();
          }
        });
    }
  }

  editProject(project: Project): void {
    this.editingProjectId = project.id!;
    this.projectForm = { ...project };
  }

  deleteProject(id: number): void {
    if (confirm('Projeyi silmek istediğinize emin misiniz?')) {
      this.portfolioService.deleteProject(id).subscribe({
        next: () => this.projects = this.projects.filter(p => p.id !== id),
        error: (err) => alert('Silme hatası: ' + err.message)
      });
    }
  }

  resetProjectForm(): void {
    this.projectForm = this.emptyProject();
    this.editingProjectId = null;
  }

  saveExperience(): void {
    if (this.editingExperienceId) {
      this.portfolioService.updateExperience(this.editingExperienceId, this.experienceForm)
        .subscribe({
          next: (updated) => {
            this.experiences = this.experiences.map(e => e.id === updated.id ? updated : e);
            this.resetExperienceForm();
          }
        });
    } else {
      this.portfolioService.createExperience(this.experienceForm)
        .subscribe({
          next: (created) => {
            this.experiences = [...this.experiences, created];
            this.resetExperienceForm();
          }
        });
    }
  }

  editExperience(exp: Experience): void {
    this.editingExperienceId = exp.id!;
    this.experienceForm = { ...exp };
  }

  deleteExperience(id: number): void {
    if (confirm('Deneyimi silmek istediğinize emin misiniz?')) {
      this.portfolioService.deleteExperience(id).subscribe({
        next: () => this.experiences = this.experiences.filter(e => e.id !== id),
        error: (err) => alert('Silme hatası: ' + err.message)
      });
    }
  }

  resetExperienceForm(): void {
    this.experienceForm = this.emptyExperience();
    this.editingExperienceId = null;
  }

  saveSkill(): void {
    if (this.editingSkillId) {
      this.portfolioService.updateSkill(this.editingSkillId, this.skillForm)
        .subscribe({
          next: (updated) => {
            this.skills = this.skills.map(s => s.id === updated.id ? updated : s);
            this.resetSkillForm();
          }
        });
    } else {
      this.portfolioService.createSkill(this.skillForm)
        .subscribe({
          next: (created) => {
            this.skills = [...this.skills, created];
            this.resetSkillForm();
          }
        });
    }
  }

  editSkill(skill: Skill): void {
    this.editingSkillId = skill.id!;
    this.skillForm = { ...skill };
  }

  deleteSkill(id: number): void {
    if (confirm('Beceriyi silmek istediğinize emin misiniz?')) {
      this.portfolioService.deleteSkill(id).subscribe({
        next: () => this.skills = this.skills.filter(s => s.id !== id),
        error: (err) => alert('Silme hatası: ' + err.message)
      });
    }
  }

  resetSkillForm(): void {
    this.skillForm = this.emptySkill();
    this.editingSkillId = null;
  }

  markAsRead(id: number): void {
    this.portfolioService.markAsRead(id).subscribe({
      next: (updated) => {
        this.messages = this.messages.map(m => m.id === updated.id ? updated : m);
      }
    });
  }

  deleteMessage(id: number): void {
    if (confirm('Mesajı silmek istediğinize emin misiniz?')) {
      this.portfolioService.deleteMessage(id).subscribe({
        next: () => this.messages = this.messages.filter(m => m.id !== id),
        error: (err) => alert('Silme hatası: ' + err.message)
      });
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/admin/login']);
  }

  private emptyProject(): Project {
    return { title: '', description: '', techStack: '', isVisible: true, orderIndex: 0 };
  }

  private emptyExperience(): Experience {
    return { company: '', position: '', description: '', startDate: new Date(), isCurrent: false, orderIndex: 0 };
  }

  private emptySkill(): Skill {
    return { name: '', category: '', level: 3, orderIndex: 0, isVisible: true };
  }
}