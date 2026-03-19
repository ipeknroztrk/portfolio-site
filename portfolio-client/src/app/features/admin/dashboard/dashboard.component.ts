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
  imports: [CommonModule, FormsModule,RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  // Aktif sekme
  activeTab: 'projects' | 'experiences' | 'skills' | 'messages' = 'projects';

  // Veriler
  projects: Project[] = [];
  experiences: Experience[] = [];
  skills: Skill[] = [];
  messages: ContactMessage[] = [];

  // Form modelleri
  projectForm: Project = this.emptyProject();
  experienceForm: Experience = this.emptyExperience();
  skillForm: Skill = this.emptySkill();

  // Düzenleme modu
  editingProjectId: number | null = null;
  editingExperienceId: number | null = null;
  editingSkillId: number | null = null;

  constructor(
    private authService: AuthService,
    private portfolioService: PortfolioService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.portfolioService.getProjects().subscribe(d => this.projects = d);
    this.portfolioService.getExperiences().subscribe(d => this.experiences = d);
    this.portfolioService.getSkills().subscribe(d => this.skills = d);
    this.portfolioService.getMessages().subscribe(d => this.messages = d);
  }

  // ── Projeler ──────────────────────────────────────────
  saveProject(): void {
    if (this.editingProjectId) {
      this.portfolioService.updateProject(this.editingProjectId, this.projectForm)
        .subscribe(() => { this.loadAll(); this.resetProjectForm(); });
    } else {
      this.portfolioService.createProject(this.projectForm)
        .subscribe(() => { this.loadAll(); this.resetProjectForm(); });
    }
  }

  editProject(project: Project): void {
    this.editingProjectId = project.id!;
    this.projectForm = { ...project };
  }

  deleteProject(id: number): void {
    if (confirm('Projeyi silmek istediğinize emin misiniz?')) {
      this.portfolioService.deleteProject(id).subscribe(() => this.loadAll());
    }
  }

  resetProjectForm(): void {
    this.projectForm = this.emptyProject();
    this.editingProjectId = null;
  }

  // ── Deneyimler ────────────────────────────────────────
  saveExperience(): void {
    if (this.editingExperienceId) {
      this.portfolioService.updateExperience(this.editingExperienceId, this.experienceForm)
        .subscribe(() => { this.loadAll(); this.resetExperienceForm(); });
    } else {
      this.portfolioService.createExperience(this.experienceForm)
        .subscribe(() => { this.loadAll(); this.resetExperienceForm(); });
    }
  }

  editExperience(exp: Experience): void {
    this.editingExperienceId = exp.id!;
    this.experienceForm = { ...exp };
  }

  deleteExperience(id: number): void {
    if (confirm('Deneyimi silmek istediğinize emin misiniz?')) {
      this.portfolioService.deleteExperience(id).subscribe(() => this.loadAll());
    }
  }

  resetExperienceForm(): void {
    this.experienceForm = this.emptyExperience();
    this.editingExperienceId = null;
  }

  // ── Beceriler ─────────────────────────────────────────
  saveSkill(): void {
    if (this.editingSkillId) {
      this.portfolioService.updateSkill(this.editingSkillId, this.skillForm)
        .subscribe(() => { this.loadAll(); this.resetSkillForm(); });
    } else {
      this.portfolioService.createSkill(this.skillForm)
        .subscribe(() => { this.loadAll(); this.resetSkillForm(); });
    }
  }

  editSkill(skill: Skill): void {
    this.editingSkillId = skill.id!;
    this.skillForm = { ...skill };
  }

  deleteSkill(id: number): void {
    if (confirm('Beceriyi silmek istediğinize emin misiniz?')) {
      this.portfolioService.deleteSkill(id).subscribe(() => this.loadAll());
    }
  }

  resetSkillForm(): void {
    this.skillForm = this.emptySkill();
    this.editingSkillId = null;
  }

  // ── Mesajlar ──────────────────────────────────────────
  markAsRead(id: number): void {
    this.portfolioService.markAsRead(id).subscribe(() => this.loadAll());
  }

  deleteMessage(id: number): void {
    if (confirm('Mesajı silmek istediğinize emin misiniz?')) {
      this.portfolioService.deleteMessage(id).subscribe(() => this.loadAll());
    }
  }

  // ── Çıkış ─────────────────────────────────────────────
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/admin/login']);
  }

  // ── Boş form modelleri ────────────────────────────────
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