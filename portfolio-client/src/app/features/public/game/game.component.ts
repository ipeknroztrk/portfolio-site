import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LanguageService } from '../../../core/services/language.service';

interface Star {
  id: number;
  x: number; y: number;
  size: number; speed: number;
  color: 'cyan' | 'silver' | 'gold' | 'powerup' | 'bomb';
  points: number;
  angle: number;
}

interface Particle {
  id: number;
  x: number; y: number;
  vx: number; vy: number;
  life: number; maxLife: number;
  color: string;
  size: number;
}

interface PowerUp {
  type: 'shield' | 'double' | 'slow';
  timeLeft: number;
}

@Component({
  selector: 'app-game',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
  levelUpMsg = false;
  shakeScreen = false;
  activePowerUp: PowerUp | null = null;
  doublePoints = false;
  slowMode = false;
  hasShield = false;
  nextLifeAt = 100;
  playerName: string = '';
  leaderboard: any[] = [];
  showNamePrompt = false;
  tempPlayerName = '';

  private gameTimer: any;
  private spawnTimer: any;
  private starId = 0;
  private particleId = 0;
  private keys = new Set<string>();
  private lastTouchX = 0;
  private frameCount = 0;

  constructor(public langService: LanguageService) {
    this.highScore = parseInt(localStorage.getItem('star_hs2') || '0');
    const savedName = localStorage.getItem('player_name');
    if (savedName) {
      this.playerName = savedName;
    }
  }

  ngOnInit(): void {
    this.loadLeaderboard();
  }
  
  ngOnDestroy(): void { 
    this.clearTimers(); 
  }

  get gameSize(): number {
    return window.innerWidth < 480 ? 320 :
           window.innerWidth < 380 ? 280 : 400;
  }

  @HostListener('window:keydown', ['$event'])
  onKeyDown(e: KeyboardEvent): void { 
    this.keys.add(e.key); 
  }

  @HostListener('window:keyup', ['$event'])
  onKeyUp(e: KeyboardEvent): void { 
    this.keys.delete(e.key); 
  }

  onTouchStart(event: TouchEvent): void {
    this.lastTouchX = event.touches[0].clientX;
    event.preventDefault();
  }

  onTouchMove(event: TouchEvent): void {
    if (!this.isPlaying) return;
    const touchX = event.touches[0].clientX;
    const diff = touchX - this.lastTouchX;
    this.lastTouchX = touchX;
    const sensitivity = this.WIDTH / window.innerWidth * 1.5;
    this.playerX = Math.max(20, Math.min(this.WIDTH - 20, this.playerX + diff * sensitivity));
    event.preventDefault();
  }

  start(): void {
    if (!this.playerName) {
      this.showNamePrompt = true;
      return;
    }
    this.initGame();
  }

  savePlayerName(): void {
    if (this.tempPlayerName.trim()) {
      this.playerName = this.tempPlayerName.trim();
      localStorage.setItem('player_name', this.playerName);
      this.showNamePrompt = false;
      this.initGame();
    }
  }

  private initGame(): void {
    this.stars = [];
    this.particles = [];
    this.playerX = 200;
    this.playerY = 350;
    this.score = 0;
    this.lives = 3;
    this.level = 1;
    this.combo = 0;
    this.comboMsg = '';
    this.levelUpMsg = false;
    this.shakeScreen = false;
    this.activePowerUp = null;
    this.doublePoints = false;
    this.slowMode = false;
    this.hasShield = false;
    this.nextLifeAt = 100;
    this.isGameOver = false;
    this.isPlaying = true;
    this.frameCount = 0;
    this.clearTimers();
    this.spawnStar();
    this.gameTimer = setInterval(() => this.tick(), 16);
  }

  private tick(): void {
    this.frameCount++;
    const speed = 6;

    if (this.keys.has('ArrowLeft') || this.keys.has('a')) {
      this.playerX = Math.max(20, this.playerX - speed);
    }
    if (this.keys.has('ArrowRight') || this.keys.has('d')) {
      this.playerX = Math.min(this.WIDTH - 20, this.playerX + speed);
    }

    const slowFactor = this.slowMode ? 0.4 : 1;

    this.stars = this.stars.map(s => ({
      ...s,
      y: s.y + s.speed * slowFactor,
      angle: s.angle + 3
    }));

    this.stars.forEach(s => {
      const dx = s.x - this.playerX;
      const dy = s.y - this.playerY;
      const dist = Math.sqrt(dx * dx + dy * dy);
      if (dist < s.size + 18) this.collect(s);
    });

    const escaped = this.stars.filter(s => s.y > this.HEIGHT + 20);
    if (escaped.length > 0) {
      const hasBomb = escaped.some(s => s.color === 'bomb');
      if (!hasBomb) {
        if (!this.hasShield) {
          this.lives -= escaped.length;
          this.triggerShake();
        } else {
          this.hasShield = false;
          this.activePowerUp = null;
        }
      }
      this.combo = 0;
      this.stars = this.stars.filter(s => s.y <= this.HEIGHT + 20);
      if (this.lives <= 0) { this.endGame(); return; }
    }

    this.particles = this.particles
      .map(p => ({ ...p, x: p.x + p.vx, y: p.y + p.vy, life: p.life - 1, vy: p.vy + 0.15, size: p.size * 0.97 }))
      .filter(p => p.life > 0);

    if (this.activePowerUp) {
      this.activePowerUp.timeLeft -= 16;
      if (this.activePowerUp.timeLeft <= 0) {
        this.doublePoints = false;
        this.slowMode = false;
        this.hasShield = false;
        this.activePowerUp = null;
      }
    }

    if (this.score >= this.nextLifeAt && this.lives < 5) {
      this.lives++;
      this.nextLifeAt += 150;
      this.showLevelUp('❤️ +1 Can!');
    }

    if (this.score >= this.level * 120) {
      this.level++;
      this.showLevelUp('⚡ LEVEL ' + this.level + '!');
    }
  }

  private collect(star: Star): void {
    this.stars = this.stars.filter(s => s.id !== star.id);

    if (star.color === 'bomb') {
      if (!this.hasShield) {
        this.lives = Math.max(0, this.lives - 1);
        this.triggerShake();
        this.spawnExplosion(star.x, star.y, '#f87171', 16);
        if (this.lives <= 0) this.endGame();
      } else {
        this.hasShield = false;
        this.activePowerUp = null;
        this.spawnExplosion(star.x, star.y, '#fbbf24', 10);
      }
      this.combo = 0;
      return;
    }

    if (star.color === 'powerup') {
      this.activatePowerUp();
      this.spawnExplosion(star.x, star.y, '#a78bfa', 12);
      this.combo++;
      return;
    }

    this.combo++;
    const multiplier = this.doublePoints ? 2 : 1;
    const comboBonus = Math.max(1, Math.floor(this.combo / 3));
    const pts = star.points * multiplier * comboBonus;
    this.score += pts;

    if (this.combo >= 10) this.comboMsg = '🔥 ULTRA x' + this.combo + '!';
    else if (this.combo >= 5) this.comboMsg = '⚡ COMBO x' + this.combo + '!';
    else this.comboMsg = '';

    const color = star.color === 'cyan' ? '#06b6d4' :
                  star.color === 'silver' ? '#c0c0c0' : '#fbbf24';
    this.spawnExplosion(star.x, star.y, color, star.color === 'gold' ? 14 : 8);

    setTimeout(() => { this.comboMsg = ''; }, 1000);
  }

  private activatePowerUp(): void {
    const types: ('shield' | 'double' | 'slow')[] = ['shield', 'double', 'slow'];
    const type = types[Math.floor(Math.random() * types.length)];
    this.activePowerUp = { type, timeLeft: 5000 };
    this.doublePoints = type === 'double';
    this.slowMode = type === 'slow';
    this.hasShield = type === 'shield';

    const msg = type === 'shield' ? '🛡️ KALKAN!' :
                type === 'double' ? '2️⃣ ÇIFT PUAN!' : '🐌 YAVAŞLAT!';
    this.showLevelUp(msg);
  }

  private spawnExplosion(x: number, y: number, color: string, count: number): void {
    for (let i = 0; i < count; i++) {
      const angle = (Math.PI * 2 / count) * i + Math.random() * 0.5;
      const speed = 1.5 + Math.random() * 3;
      this.particles.push({
        id: this.particleId++,
        x, y,
        vx: Math.cos(angle) * speed,
        vy: Math.sin(angle) * speed - 1,
        life: 25 + Math.random() * 20,
        maxLife: 45,
        color,
        size: 2 + Math.random() * 3
      });
    }
  }

  private triggerShake(): void {
    this.shakeScreen = true;
    setTimeout(() => this.shakeScreen = false, 400);
  }

  private showLevelUp(msg: string): void {
    this.comboMsg = msg;
    this.levelUpMsg = true;
    setTimeout(() => { this.comboMsg = ''; this.levelUpMsg = false; }, 1500);
  }

  private spawnStar(): void {
    const rand = Math.random();
    let color: Star['color'];
    let points: number;
    let size: number;

    if (rand > 0.97) {
      color = 'powerup'; points = 0; size = 14;
    } else if (rand > 0.93) {
      color = 'bomb'; points = 0; size = 13;
    } else if (rand > 0.85) {
      color = 'gold'; points = 30; size = 14;
    } else if (rand > 0.65) {
      color = 'silver'; points = 20; size = 12;
    } else {
      color = 'cyan'; points = 10; size = 10;
    }

    this.stars.push({
      id: this.starId++,
      x: 20 + Math.random() * (this.WIDTH - 40),
      y: -20,
      size, color, points, angle: 0,
      speed: 1.2 + this.level * 0.25 + Math.random() * 1.2
    });

    const delay = Math.max(300, 1100 - this.level * 70);
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
      localStorage.setItem('star_hs2', String(this.highScore));
    }

    this.sendScore();
    this.loadLeaderboard();
  }

  private clearTimers(): void {
    clearInterval(this.gameTimer);
    clearTimeout(this.spawnTimer);
  }

  getLives(): number[] { 
    return Array(Math.max(0, this.lives)).fill(0); 
  }

  getPowerUpColor(): string {
    if (!this.activePowerUp) return '';
    return this.activePowerUp.type === 'shield' ? '#60a5fa' :
           this.activePowerUp.type === 'double' ? '#fbbf24' : '#34d399';
  }

  getPowerUpProgress(): number {
    if (!this.activePowerUp) return 0;
    return (this.activePowerUp.timeLeft / 5000) * 100;
  }

  getStarColor(star: Star): string {
    if (star.color === 'powerup') return '#a78bfa';
    if (star.color === 'bomb') return '#f87171';
    if (star.color === 'cyan') return '#06b6d4';
    if (star.color === 'silver') return '#c0c0c0';
    return '#fbbf24';
  }

  getStarFilter(star: Star): string {
    if (star.color === 'powerup') return 'url(#glow-powerup)';
    if (star.color === 'bomb') return 'url(#glow-bomb)';
    return 'url(#glow-' + star.color + ')';
  }

  async sendScore() {
    try {
      await fetch('https://api.ipekozturk.com/api/leaderboard', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          playerName: this.playerName,
          points: this.score
        })
      });
    } catch (error) {
      console.error('Score gönderilemedi:', error);
    }
  }

  async loadLeaderboard() {
    try {
      const response = await fetch('https://api.ipekozturk.com/api/leaderboard');
      const data = await response.json();
      this.leaderboard = data.sort((a: any, b: any) => b.points - a.points);
    } catch (error) {
      console.error('Leaderboard yüklenemedi:', error);
      this.leaderboard = [];
    }
  }

  getStarPath(size: number, isBomb = false): string {
    if (isBomb) {
      let path = '';
      for (let i = 0; i < 8; i++) {
        const a = (i * Math.PI * 2) / 8 - Math.PI / 8;
        path += (i === 0 ? 'M' : 'L') + (size * Math.cos(a)).toFixed(2) + ',' + (size * Math.sin(a)).toFixed(2);
      }
      return path + 'Z';
    }
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