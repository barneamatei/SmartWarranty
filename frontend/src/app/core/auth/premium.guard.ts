import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, tap } from 'rxjs';

import { UiFeedbackService } from '../../shared/ui/ui-feedback.service';
import { PremiumAccessService } from './premium-access.service';

export const premiumGuard: CanActivateFn = () => {
  const premiumAccess = inject(PremiumAccessService);
  const router = inject(Router);
  const uiFeedback = inject(UiFeedbackService);

  return premiumAccess.checkPremium().pipe(
    tap((isPremium) => {
      if (!isPremium) {
        uiFeedback.error('Ai nevoie de cont Premium pentru aceasta sectiune.');
      }
    }),
    map((isPremium) => (isPremium ? true : router.createUrlTree(['/subscriptions'])))
  );
};
