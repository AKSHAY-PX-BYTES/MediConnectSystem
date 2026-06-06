import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/auth.models';

export const authGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  const allowedRoles = route.data?.['roles'] as UserRole[] | undefined;
  if (allowedRoles && allowedRoles.length > 0) {
    const role = auth.role();
    if (!role || !allowedRoles.includes(role)) {
      return router.createUrlTree(['/app/dashboard']);
    }
  }

  return true;
};
