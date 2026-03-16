import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Token varsa geçir, yoksa login'e yönlendir
  if (authService.isLoggedIn()) {
    return true;
  }

  router.navigate(['/admin/login']);
  return false;
};