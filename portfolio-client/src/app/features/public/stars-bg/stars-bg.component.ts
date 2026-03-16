import { Component, AfterViewInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';

interface Star {
  x: number; y: number;
  size: number; speed: number;
  phase: number;
  type: 'cyan' | 'silver' | 'white' | 'gold';
}

interface Meteor {
  x: number; y: number;
  vx: number; vy: number;
  length: number;
  life: number; maxLife: number;
}

interface MatrixDrop {
  x: number; y: number;
  char: string; speed: number;
  opacity: number; life: number; maxLife: number;
}

@Component({
  selector: 'app-stars-bg',
  standalone: true,
  template: `<canvas #canvas class="stars-canvas"></canvas>`,
  styles: [`
    .stars-canvas {
      position: fixed;
      top: 0; left: 0;
      width: 100%; height: 100%;
      pointer-events: none;
      z-index: 0;
    }
  `]
})
export class StarsBgComponent implements AfterViewInit, OnDestroy {
  @ViewChild('canvas') canvasRef!: ElementRef<HTMLCanvasElement>;

  private ctx!: CanvasRenderingContext2D;
  private stars: Star[] = [];
  private meteors: Meteor[] = [];
  private matrixDrops: MatrixDrop[] = [];
  private animId!: number;
  private resizeObs!: ResizeObserver;
  private meteorTimer: any;

  ngAfterViewInit(): void {
    const canvas = this.canvasRef.nativeElement;
    this.ctx = canvas.getContext('2d')!;
    this.resize(canvas);
    this.initStars(canvas);
    this.animate(canvas);
    this.scheduleMeteor(canvas);

    this.resizeObs = new ResizeObserver(() => {
      this.resize(canvas);
      this.initStars(canvas);
    });
    this.resizeObs.observe(document.body);
  }

  ngOnDestroy(): void {
    cancelAnimationFrame(this.animId);
    clearTimeout(this.meteorTimer);
    this.resizeObs?.disconnect();
  }

  private resize(canvas: HTMLCanvasElement): void {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
  }

  private initStars(canvas: HTMLCanvasElement): void {
    this.stars = [];
    // Çok daha fazla yıldız
    const count = Math.floor((canvas.width * canvas.height) / 1800);
    const types: ('cyan' | 'silver' | 'white' | 'gold')[] = ['cyan', 'silver', 'white', 'gold'];

    for (let i = 0; i < count; i++) {
      const rand = Math.random();
      // Boyut dağılımı — çoğu küçük, bazıları büyük, nadiren dev
      const size = rand > 0.99 ? 4 + Math.random() * 3   // dev yıldızlar %1
                 : rand > 0.95 ? 2.5 + Math.random() * 2  // büyük %4
                 : rand > 0.85 ? 1.2 + Math.random() * 1.3 // orta %10
                 : rand > 0.6  ? 0.6 + Math.random() * 0.6 // küçük %25
                 :               0.2 + Math.random() * 0.4; // minik %60

      // Tip dağılımı
      const typeRand = Math.random();
      const type = typeRand > 0.85 ? 'cyan'
                 : typeRand > 0.7  ? 'silver'
                 : typeRand > 0.98 ? 'gold'
                 :                   'white';

      this.stars.push({
        x: Math.random() * canvas.width,
        y: Math.random() * canvas.height,
        size, type,
        speed: Math.random() * 0.006 + 0.001,
        phase: Math.random() * Math.PI * 2
      });
    }
  }

  private scheduleMeteor(canvas: HTMLCanvasElement): void {
    // Daha sık meteor — 1.5-4 saniye
    const delay = 1500 + Math.random() * 2500;
    this.meteorTimer = setTimeout(() => {
      // Bazen çift meteor!
      this.spawnMeteor(canvas);
      if (Math.random() > 0.6) {
        setTimeout(() => this.spawnMeteor(canvas), 300 + Math.random() * 400);
      }
      this.scheduleMeteor(canvas);
    }, delay);
  }

  private spawnMeteor(canvas: HTMLCanvasElement): void {
    const x = Math.random() * canvas.width * 0.8 + canvas.width * 0.2;
    const angle = Math.PI / 4 + (Math.random() - 0.5) * 0.5;
    const speed = 10 + Math.random() * 8;

    this.meteors.push({
      x, y: -20,
      vx: -Math.cos(angle) * speed,
      vy: Math.sin(angle) * speed,
      length: 100 + Math.random() * 150,
      life: 0,
      maxLife: 50 + Math.random() * 40
    });
  }

  private spawnMatrixDrop(canvas: HTMLCanvasElement): void {
    const chars = '01アイウエカキクネコ∑∆∫√∞</>{}[]';
    this.matrixDrops.push({
      x: Math.random() * canvas.width,
      y: 0,
      char: chars[Math.floor(Math.random() * chars.length)],
      speed: 1.5 + Math.random() * 2.5,
      opacity: 0.2 + Math.random() * 0.35,
      life: 0,
      maxLife: 100 + Math.random() * 80
    });
  }

  private animate(canvas: HTMLCanvasElement): void {
    this.ctx.clearRect(0, 0, canvas.width, canvas.height);
    const now = Date.now() * 0.001;

    // ── Yıldızlar ──────────────────────────────────────────
    this.stars.forEach(star => {
      const opacity = (Math.sin(now * star.speed * 10 + star.phase) + 1) / 2;

      let finalOpacity: number;
      let color: string;
      let glowColor: string;
      let glowBlur = 0;

      switch (star.type) {
        case 'cyan':
          finalOpacity = 0.2 + opacity * 0.8;
          color = `rgba(6, 182, 212, ${finalOpacity})`;
          glowColor = 'rgba(6, 182, 212, 0.9)';
          glowBlur = star.size > 1.2 ? star.size * 4 : 0;
          break;
        case 'silver':
          finalOpacity = 0.15 + opacity * 0.7;
          color = `rgba(192, 192, 192, ${finalOpacity})`;
          glowColor = 'rgba(192, 192, 192, 0.7)';
          glowBlur = star.size > 1.5 ? star.size * 3 : 0;
          break;
        case 'gold':
          finalOpacity = 0.3 + opacity * 0.7;
          color = `rgba(251, 191, 36, ${finalOpacity})`;
          glowColor = 'rgba(251, 191, 36, 0.8)';
          glowBlur = star.size > 1 ? star.size * 5 : 0;
          break;
        default:
          finalOpacity = 0.1 + opacity * 0.6;
          color = `rgba(226, 232, 240, ${finalOpacity})`;
          glowColor = 'rgba(226, 232, 240, 0.5)';
          glowBlur = star.size > 2 ? star.size * 2 : 0;
      }

      this.ctx.beginPath();
      this.ctx.arc(star.x, star.y, star.size, 0, Math.PI * 2);
      this.ctx.fillStyle = color;

      if (glowBlur > 0) {
        this.ctx.shadowBlur = glowBlur;
        this.ctx.shadowColor = glowColor;
      }

      this.ctx.fill();
      this.ctx.shadowBlur = 0;

      // Büyük yıldızlara çapraz ışın efekti
      if (star.size > 3) {
        this.ctx.strokeStyle = color;
        this.ctx.lineWidth = 0.5;
        this.ctx.globalAlpha = finalOpacity * 0.3;
        this.ctx.beginPath();
        this.ctx.moveTo(star.x - star.size * 3, star.y);
        this.ctx.lineTo(star.x + star.size * 3, star.y);
        this.ctx.moveTo(star.x, star.y - star.size * 3);
        this.ctx.lineTo(star.x, star.y + star.size * 3);
        this.ctx.stroke();
        this.ctx.globalAlpha = 1;
      }
    });

    // ── Meteorlar ──────────────────────────────────────────
    this.meteors = this.meteors.filter(m => m.life < m.maxLife);
    this.meteors.forEach(m => {
      m.x += m.vx;
      m.y += m.vy;
      m.life++;

      const progress = m.life / m.maxLife;
      const opacity = progress < 0.2
        ? progress / 0.2
        : 1 - (progress - 0.2) / 0.8;

      const tailX = m.x - m.vx / Math.abs(m.vy) * m.length;
      const tailY = m.y - m.length;

      const gradient = this.ctx.createLinearGradient(m.x, m.y, tailX, tailY);
      gradient.addColorStop(0, `rgba(255, 255, 255, ${opacity})`);
      gradient.addColorStop(0.2, `rgba(6, 182, 212, ${opacity * 0.9})`);
      gradient.addColorStop(0.6, `rgba(192, 192, 192, ${opacity * 0.4})`);
      gradient.addColorStop(1, 'rgba(6, 182, 212, 0)');

      this.ctx.beginPath();
      this.ctx.moveTo(m.x, m.y);
      this.ctx.lineTo(tailX, tailY);
      this.ctx.strokeStyle = gradient;
      this.ctx.lineWidth = 2;
      this.ctx.shadowBlur = 12;
      this.ctx.shadowColor = 'rgba(6, 182, 212, 0.8)';
      this.ctx.stroke();
      this.ctx.shadowBlur = 0;

      // Meteor başına parlak nokta
      this.ctx.beginPath();
      this.ctx.arc(m.x, m.y, 2, 0, Math.PI * 2);
      this.ctx.fillStyle = `rgba(255, 255, 255, ${opacity})`;
      this.ctx.shadowBlur = 8;
      this.ctx.shadowColor = '#fff';
      this.ctx.fill();
      this.ctx.shadowBlur = 0;
    });

    // ── Matrix damlacıkları ────────────────────────────────
    if (Math.random() > 0.993) {
      this.spawnMatrixDrop(canvas);
    }

    this.matrixDrops = this.matrixDrops.filter(d => d.life < d.maxLife);
    this.matrixDrops.forEach(d => {
      d.y += d.speed;
      d.life++;

      const progress = d.life / d.maxLife;
      const opacity = progress < 0.15
        ? progress / 0.15
        : 1 - (progress - 0.15) / 0.85;

      this.ctx.font = '11px "Fira Code", monospace';
      this.ctx.fillStyle = `rgba(6, 182, 212, ${opacity * d.opacity})`;
      this.ctx.shadowBlur = 4;
      this.ctx.shadowColor = 'rgba(6, 182, 212, 0.4)';
      this.ctx.fillText(d.char, d.x, d.y);
      this.ctx.shadowBlur = 0;
    });

    this.animId = requestAnimationFrame(() => this.animate(canvas));
  }
}