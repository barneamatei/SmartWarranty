import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { AuthViewModel } from '../../features/auth/view-models/auth.view-model';

export const authGuard: CanActivateFn = () => {
  const authViewModel = inject(AuthViewModel);
  const router = inject(Router);

  if (authViewModel.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/auth/login']);
};
