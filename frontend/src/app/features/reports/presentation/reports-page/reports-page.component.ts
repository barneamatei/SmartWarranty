import { Component, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { apiConfig } from '../../../../core/config/api.config';
import { AuthViewModel } from '../../../auth/view-models/auth.view-model';
import { DashboardApiService } from '../../../dashboard/data/dashboard-api.service';
import { ReportPreview } from '../../../dashboard/models/dashboard.models';

@Component({
  selector: 'app-reports-page',
  imports: [MatButtonModule, MatCardModule, MatProgressSpinnerModule],
  templateUrl: './reports-page.component.html',
  styleUrl: './reports-page.component.scss'
})
export class ReportsPageComponent {
  private readonly authViewModel = inject(AuthViewModel);
  private readonly dashboardApi = inject(DashboardApiService);
  readonly loading = signal(true);
  readonly portfolioReport = signal<ReportPreview | null>(null);
  readonly expiringReport = signal<ReportPreview | null>(null);

  constructor() {
    const userId = this.currentUserId();

    this.dashboardApi
      .getSnapshot(userId)
      .pipe(takeUntilDestroyed())
      .subscribe((snapshot) => {
        this.portfolioReport.set(snapshot.portfolioReport);
        this.expiringReport.set(snapshot.expiringReport);
        this.loading.set(false);
      });
  }

  exportReport(report: 'portfolio' | 'expiring-warranties', format: 'pdf' | 'xlsx') {
    const userId = this.currentUserId();
    const params = new URLSearchParams({ format });

    if (report === 'expiring-warranties') {
      params.set('daysAhead', '30');
    }

    if (userId) {
      params.set('userId', userId);
    }

    window.open(`${apiConfig.gatewayBaseUrl}/reports/api/reports/${report}/export?${params.toString()}`, '_blank', 'noopener,noreferrer');
  }

  private currentUserId() {
    return this.authViewModel.user()?.userId || this.authViewModel.user()?.id;
  }
}
