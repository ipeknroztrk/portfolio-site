import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app.component';
import { PortfolioService } from './core/services/portfolio.service';

describe('AppComponent', () => {
  let portfolioService: jasmine.SpyObj<PortfolioService>;

  beforeEach(async () => {
    portfolioService = jasmine.createSpyObj<PortfolioService>('PortfolioService', ['wakeUpApi']);

    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideRouter([]),
        { provide: PortfolioService, useValue: portfolioService }
      ]
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should render the router outlet', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).querySelector('router-outlet')).not.toBeNull();
  });

  it('should wake up the API on init (cold start on Render)', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    expect(portfolioService.wakeUpApi).toHaveBeenCalledTimes(1);
  });
});
