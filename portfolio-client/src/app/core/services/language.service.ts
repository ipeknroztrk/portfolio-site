import { Injectable, signal } from '@angular/core';

export type Lang = 'tr' | 'en';

export const TRANSLATIONS = {
  tr: {
    nav: {
      about: 'Hakkımda',
      experience: 'Deneyim',
      projects: 'Projeler',
      skills: 'Beceriler',
      contact: 'İletişim',
    },
    hero: {
      greeting: '// Merhaba, ben',
      titles: ['Full Stack Developer', '.NET Backend Developer'],
      desc: 'ASP.NET Core, Entity Framework Core ve Angular ile modern, ölçeklenebilir web uygulamaları geliştiriyorum. Temiz mimari ve güvenli kod önceliğimdir.',
      cta1: 'Projelerimi Gör',
      cta2: 'İletişime Geç',
    },
    about: {
      tag: '// hakkımda',
      title: 'Merhaba, ben İpek 👋',
      p1: 'Trakya Üniversitesi Bilgisayar Mühendisliği mezunuyum (Haziran 2025). ASP.NET Core, Entity Framework Core ve PostgreSQL ile backend geliştirme konusunda güçlü pratik deneyime sahibim.',
      p2: 'VakıfBank\'ta iki kez .NET stajı yaptım — hazine işlemleri ve ödeme sistemleri alanında gerçek dünya projeleri geliştirdim. BilgeAdam\'da full-stack eğitimi ile Angular ve React\'e de hakimim.',
      p3: 'Temiz mimari, güvenli kod ve sürdürülebilir sistemler kurmak temel önceliğimdir. Hızlı öğrenen, takım uyumlu bir geliştirici olarak yeni fırsatlar arıyorum.',
      location: 'Konum',
      email: 'E-posta',
      status: 'Durum',
      language: 'Dil',
      statusVal: 'İş Arıyorum',
      langVal: 'Türkçe (Ana dil), İngilizce (Orta)',
      cvBtn: 'CV İndir',
      stats: [
        { value: '2+', label: 'Yıl Deneyim' },
        { value: '5+', label: 'Proje' },
        { value: '3', label: 'Staj' },
        { value: '3.10', label: 'GPA' },
      ]
    },
    experience: {
      tag: '// deneyim',
      title: 'Deneyim & Eğitim',
      work: 'İş Deneyimi',
      education: 'Eğitim & Bootcamp',
      current: 'Devam ediyor',
      present: 'Günümüz',
      uniDegree: 'Bilgisayar Mühendisliği',
      uniDetail: 'B.Sc. · GPA: 3.10 · Haziran 2025 mezunu',
    },
    projects: {
      tag: '// projeler',
      title: 'Öne Çıkan Projeler',
      detail: 'Detayları gör →',
      live: 'Canlıya Git',
      github: 'GitHub\'da Gör',
    },
    skills: {
      tag: '// beceriler',
      title: 'Teknik Beceriler',
      levels: { 1: 'Başlangıç', 2: 'Temel', 3: 'Orta', 4: 'İleri', 5: 'Uzman' }
    },
    contact: {
      tag: '// iletişim',
      title: 'Birlikte Çalışalım',
      desc: 'Yeni bir proje, iş fırsatı veya sadece merhaba demek için mesaj gönderebilirsiniz.',
      name: 'Ad Soyad',
      namePh: 'Adınız Soyadınız',
      emailPh: 'mail@example.com',
      message: 'Mesaj',
      messagePh: 'Mesajınız...',
      send: 'Mesaj Gönder →',
      sending: 'Gönderiliyor...',
      success: 'Mesajınız iletildi, en kısa sürede dönüş yapacağım!',
      error: 'Bir hata oluştu, lütfen tekrar deneyin.',
      validationError: 'Lütfen tüm alanları doldurun.',
      emailError: 'Geçerli bir e-posta adresi girin.',
    }
  },
  en: {
    nav: {
      about: 'About',
      experience: 'Experience',
      projects: 'Projects',
      skills: 'Skills',
      contact: 'Contact',
    },
    hero: {
      greeting: '// Hello, I\'m',
      titles: ['Full Stack Developer', '.NET Backend Developer'],
      desc: 'I build modern, scalable web applications with ASP.NET Core, Entity Framework Core and Angular. Clean architecture and secure code are my priorities.',
      cta1: 'View Projects',
      cta2: 'Get In Touch',
    },
    about: {
      tag: '// about',
      title: 'Hi, I\'m İpek 👋',
      p1: 'I\'m a Computer Engineering graduate from Trakya University (June 2025). I have strong hands-on experience in backend development with ASP.NET Core, Entity Framework Core, and PostgreSQL.',
      p2: 'I completed two .NET internships at VakıfBank — developing real-world projects in treasury operations and payment systems. Through BilgeAdam\'s full-stack training, I also gained solid skills in Angular and React.',
      p3: 'Clean architecture, secure code, and building maintainable systems are my top priorities. I\'m a fast learner and team-oriented developer looking for new opportunities.',
      location: 'Location',
      email: 'Email',
      status: 'Status',
      language: 'Language',
      statusVal: 'Open to Work',
      langVal: 'Turkish (Native), English (Intermediate)',
      cvBtn: 'Download CV',
      stats: [
        { value: '2+', label: 'Years Exp.' },
        { value: '5+', label: 'Projects' },
        { value: '3', label: 'Internships' },
        { value: '3.10', label: 'GPA' },
      ]
    },
    experience: {
      tag: '// experience',
      title: 'Experience & Education',
      work: 'Work Experience',
      education: 'Education & Bootcamps',
      current: 'Current',
      present: 'Present',
      uniDegree: 'Computer Engineering',
      uniDetail: 'B.Sc. · GPA: 3.10 · June 2025 graduate',
    },
    projects: {
      tag: '// projects',
      title: 'Featured Projects',
      detail: 'View details →',
      live: 'Live Demo',
      github: 'View on GitHub',
    },
    skills: {
      tag: '// skills',
      title: 'Technical Skills',
      levels: { 1: 'Beginner', 2: 'Basic', 3: 'Intermediate', 4: 'Advanced', 5: 'Expert' }
    },
    contact: {
      tag: '// contact',
      title: 'Let\'s Work Together',
      desc: 'Whether you have a project, a job opportunity, or just want to say hi — send me a message.',
      name: 'Full Name',
      namePh: 'Your Full Name',
      emailPh: 'mail@example.com',
      message: 'Message',
      messagePh: 'Your message...',
      send: 'Send Message →',
      sending: 'Sending...',
      success: 'Your message has been sent! I\'ll get back to you soon.',
      error: 'Something went wrong. Please try again.',
      validationError: 'Please fill in all fields.',
      emailError: 'Please enter a valid email address.',
    }
  }
};

@Injectable({ providedIn: 'root' })
export class LanguageService {
  current = signal<Lang>('tr');

  constructor() {
    const saved = localStorage.getItem('lang') as Lang;
    if (saved === 'tr' || saved === 'en') {
      this.current.set(saved);
    }
  }

  toggle(): void {
    this.current.set(this.current() === 'tr' ? 'en' : 'tr');
    localStorage.setItem('lang', this.current());
  }

  get t() {
    return TRANSLATIONS[this.current()];
  }
}