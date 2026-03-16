import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-cursor',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cursor.component.html',
  styleUrl: './cursor.component.scss'
})
export class CursorComponent implements OnInit, OnDestroy {
  x = -100;
  y = -100;
  trailX = -100;
  trailY = -100;
  isHovering = false;
  isClicking = false;
  private animId!: number;

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    window.addEventListener('mousemove', this.onMove);
    window.addEventListener('mouseover', this.onOver);
    window.addEventListener('mousedown', this.onDown);
    window.addEventListener('mouseup', this.onUp);
    this.animate();
  }

  ngOnDestroy(): void {
    window.removeEventListener('mousemove', this.onMove);
    window.removeEventListener('mouseover', this.onOver);
    window.removeEventListener('mousedown', this.onDown);
    window.removeEventListener('mouseup', this.onUp);
    cancelAnimationFrame(this.animId);
  }

  private onMove = (e: MouseEvent) => {
    this.x = e.clientX;
    this.y = e.clientY;
  };

  private onOver = (e: MouseEvent) => {
    const t = e.target as HTMLElement;
    this.isHovering = !!(t.closest('a') || t.closest('button') || t.closest('.project-card') || t.closest('.stat-card'));
  };

  private onDown = () => { this.isClicking = true; };
  private onUp = () => { this.isClicking = false; };

  private animate(): void {
    this.trailX += (this.x - this.trailX) * 0.1;
    this.trailY += (this.y - this.trailY) * 0.1;
    this.cdr.detectChanges();
    this.animId = requestAnimationFrame(() => this.animate());
  }
}