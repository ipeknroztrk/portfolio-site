import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from '../../../core/services/language.service';

interface Star {
  id: number;
  x: number;
  y: number;
  size: number;
  speed: number;
  color: 'cyan' | 'silver' | 'gold';
  points: number;
  angle: number;
}

interface Particle {
  id: number;
  x: number; y: number;
  vx: number; vy: number;
  life: number;
  color: string;
}

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './game.component.html',
  styleUrl: './game.component.scss'
})
export class GameComponent implements OnInit, OnDestroy {
  readonly WIDTH = 400;
  readonly HEIGHT = 400;

  stars: Star[] = [];
  particles: Particle[] = [];
  playerX = 200;
  playerY = 350;
  score = 0;
  highScore = 0;
  lives = 3;
  level = 1;
  isPlaying = false;
  isGameOver = false;
  combo = 0;
  comboMsg = '';
  private gameTimer: any;
  private spawnTimer: any;
  private starId = 0;
  private particleId = 0;
  private keys = new Set<string>();

  constructor(public langService: LanguageService) {
    this.highScore = parseInt(localStorage.getItem('star_hs') || '0');
  }

  ngOnInit(): void {}

  ngOnDestroy(): void {
    this.clearTimers();
  }

  @HostListener('window:keydown', ['$event'])
  onKeyDown(e: KeyboardEvent): void {
    this.keys.add(e.key);
  }
  get gameSize(): number {
    return window.innerWidth < 480 ? 320 :
           window.innerWidth < 380 ? 280 : 400;
  }
  @HostListener('window:keyup', ['$event'])
  onKeyUp(e: KeyboardEvent): void {
    this.keys.delete(e.key);
  }
  private lastTouchX: number = 0;

onTouchStart(event: TouchEvent): void {
  this.lastTouchX = event.touches[0].clientX;
  event.preventDefault();
}

onTouchMove(event: TouchEvent): void {
  if (!this.isPlaying) return;
  const touchX = event.touches[0].clientX;
  const diff = touchX - this.lastTouchX;
  this.lastTouchX = touchX;

  // Sürükleme hareketini oyun alanına oranla hesapla
  const sensitivity = this.WIDTH / window.innerWidth * 1.5;
  this.playerX = Math.max(20, Math.min(this.WIDTH - 20,
    this.playerX + diff * sensitivity));

  event.preventDefault();
}

  start(): void {
    this.stars = [];
    this.particles = [];
    this.playerX = 200;
    this.playerY = 350;
    this.score = 0;
    this.lives = 3;
    this.level = 1;
    this.combo = 0;
    this.comboMsg = '';
    this.isGameOver = false;
    this.isPlaying = true;
    this.clearTimers();
    this.spawnStar();
    this.gameTimer = setInterval(() => this.tick(), 16);
  }

  private tick(): void {
    // Oyuncu hareketi
    const speed = 5;
    if (this.keys.has('ArrowLeft') || this.keys.has('a')) {
      this.playerX = Math.max(20, this.playerX - speed);
    }
    if (this.keys.has('ArrowRight') || this.keys.has('d')) {
      this.playerX = Math.min(this.WIDTH - 20, this.playerX + speed);
    }

    // Yıldızları hareket ettir
    this.stars = this.stars.map(s => ({
      ...s,
      y: s.y + s.speed,
      angle: s.angle + 2
    }));

    // Çarpışma kontrolü
    this.stars.forEach(s => {
      const dx = s.x - this.playerX;
      const dy = s.y - this.playerY;
      const dist = Math.sqrt(dx * dx + dy * dy);

      if (dist < s.size + 18) {
        // Yıldız toplandı!
        this.collect(s);
      }
    });

    // Ekrandan çıkan yıldızlar — can düşür
    const escaped = this.stars.filter(s => s.y > this.HEIGHT + 20);
    if (escaped.length > 0) {
      this.lives -= escaped.length;
      this.combo = 0;
      this.comboMsg = '';
      this.stars = this.stars.filter(s => s.y <= this.HEIGHT + 20);

      if (this.lives <= 0) {
        this.endGame();
        return;
      }
    }

    // Parçacıkları güncelle
    this.particles = this.particles
      .map(p => ({ ...p, x: p.x + p.vx, y: p.y + p.vy, life: p.life - 1, vy: p.vy + 0.1 }))
      .filter(p => p.life > 0);

    // Level atlama
    if (this.score >= this.level * 100) {
      this.level++;
    }
  }

  private collect(star: Star): void {
    this.stars = this.stars.filter(s => s.id !== star.id);
    this.combo++;
    const pts = star.points * Math.max(1, Math.floor(this.combo / 3));
    this.score += pts;

    if (this.combo >= 5) this.comboMsg = 'COMBO x' + this.combo + '! 🔥';
    else this.comboMsg = '';

    // Parçacık efekti
    const color = star.color === 'cyan' ? '#06b6d4' : star.color === 'silver' ? '#c0c0c0' : '#fbbf24';
    for (let i = 0; i < 8; i++) {
      this.particles.push({
        id: this.particleId++,
        x: star.x, y: star.y,
        vx: (Math.random() - 0.5) * 4,
        vy: (Math.random() - 0.5) * 4 - 2,
        life: 20 + Math.random() * 15,
        color
      });
    }

    setTimeout(() => { this.comboMsg = ''; }, 800);
  }

  private spawnStar(): void {
    const colors: ('cyan' | 'silver' | 'gold')[] = ['cyan', 'cyan', 'cyan', 'silver', 'silver', 'gold'];
    const color = colors[Math.floor(Math.random() * colors.length)];
    const points = color === 'gold' ? 30 : color === 'silver' ? 20 : 10;

    this.stars.push({
      id: this.starId++,
      x: 20 + Math.random() * (this.WIDTH - 40),
      y: -20,
      size: color === 'gold' ? 14 : color === 'silver' ? 12 : 10,
      speed: 1.5 + this.level * 0.3 + Math.random() * 1.5,
      color, points, angle: 0
    });

    const delay = Math.max(400, 1200 - this.level * 80);
    this.spawnTimer = setTimeout(() => {
      if (this.isPlaying) this.spawnStar();
    }, delay);
  }

  private endGame(): void {
    this.clearTimers();
    this.isPlaying = false;
    this.isGameOver = true;
    if (this.score > this.highScore) {
      this.highScore = this.score;
      localStorage.setItem('star_hs', String(this.highScore));
    }
  }

  private clearTimers(): void {
    clearInterval(this.gameTimer);
    clearTimeout(this.spawnTimer);
  }

  movePlayer(dx: number): void {
    this.playerX = Math.max(20, Math.min(this.WIDTH - 20, this.playerX + dx * 30));
  }

  getLives(): number[] {
    return Array(Math.max(0, this.lives)).fill(0);
  }

  getStarPath(size: number): string {
    const pts = 5;
    const outer = size;
    const inner = size * 0.4;
    let path = '';
    for (let i = 0; i < pts * 2; i++) {
      const r = i % 2 === 0 ? outer : inner;
      const a = (i * Math.PI) / pts - Math.PI / 2;
      path += (i === 0 ? 'M' : 'L') + (r * Math.cos(a)).toFixed(2) + ',' + (r * Math.sin(a)).toFixed(2);
    }
    return path + 'Z';
  }
}