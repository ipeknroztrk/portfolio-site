import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PortfolioService } from './core/services/portfolio.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet />`
})
export class AppComponent implements OnInit {
  constructor(private portfolioService: PortfolioService) {}

  ngOnInit(): void {
    this.portfolioService.wakeUpApi();
  }
}
