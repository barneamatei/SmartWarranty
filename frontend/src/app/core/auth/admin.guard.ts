import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { UiFeedbackService } from '../../shared/ui/ui-feedback.service';
import { AuthViewModel } from '../../features/auth/view-models/auth.view-model';

export const adminGuard: CanActivateFn = () => {
  const authViewModel = inject(AuthViewModel);
  const router = inject(Router);
  const uiFeedback = inject(UiFeedbackService);
  const user = authViewModel.user();
  const roles = [user?.role, ...(user?.roles ?? [])].filter(Boolean).map((role) => role!.toLowerCase());

  if (roles.includes('admin') || roles.includes('administrator')) {
    return true;
  }

  uiFeedback.error('Ai nevoie de cont admin pentru aceasta sectiune.');
  return router.createUrlTree(['/dashboard']);
};
