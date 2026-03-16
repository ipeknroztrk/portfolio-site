import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PortfolioService } from '../../../core/services/portfolio.service';
import { ContactMessage } from '../../../core/models/contact.model';
import { LanguageService } from '../../../core/services/language.service';
import { FadeUpDirective } from '../../../core/directives/fade-up.directive';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, FormsModule, FadeUpDirective],
  templateUrl: './contact.component.html',
  styleUrl: './contact.component.scss'
})
export class ContactComponent {
  form: ContactMessage = { fullName: '', email: '', message: '' };
  isSending = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private portfolioService: PortfolioService,
    public langService: LanguageService
  ) {}

  onSubmit(): void {
    const t = this.langService.t.contact;

    if (!this.form.fullName || !this.form.email || !this.form.message) {
      this.errorMessage = t.validationError;
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.form.email)) {
      this.errorMessage = t.emailError;
      return;
    }

    this.isSending = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.portfolioService.sendMessage(this.form).subscribe({
      next: () => {
        this.successMessage = t.success;
        this.form = { fullName: '', email: '', message: '' };
        this.isSending = false;
      },
      error: () => {
        this.errorMessage = t.error;
        this.isSending = false;
      }
    });
  }
}