import { Routes } from '@angular/router';

import { adminGuard } from './core/auth/admin.guard';
import { authGuard } from './core/auth/auth.guard';
import { guestGuard } from './core/auth/guest.guard';
import { premiumGuard } from './core/auth/premium.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard'
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./core/shell/app-shell.component').then((module) => module.AppShellComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/presentation/dashboard-page/dashboard-page.component').then(
            (module) => module.DashboardPageComponent
          )
      },
      {
        path: 'admin',
        canActivate: [adminGuard],
        loadComponent: () =>
          import('./features/admin/presentation/admin-dashboard-page/admin-dashboard-page.component').then(
            (module) => module.AdminDashboardPageComponent
          )
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./features/products/presentation/products-page/products-page.component').then(
            (module) => module.ProductsPageComponent
          )
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./features/categories/presentation/categories-page/categories-page.component').then(
            (module) => module.CategoriesPageComponent
          )
      },
      {
        path: 'warranties',
        loadComponent: () =>
          import('./features/warranties/presentation/warranties-page/warranties-page.component').then(
            (module) => module.WarrantiesPageComponent
          )
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/notifications/presentation/notifications-page/notifications-page.component').then(
            (module) => module.NotificationsPageComponent
          )
      },
      {
        path: 'documents',
        loadComponent: () =>
          import('./features/documents/presentation/documents-page/documents-page.component').then(
            (module) => module.DocumentsPageComponent
          )
      },
      {
        path: 'reports',
        canActivate: [premiumGuard],
        loadComponent: () =>
          import('./features/reports/presentation/reports-page/reports-page.component').then(
            (module) => module.ReportsPageComponent
          )
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./features/account/presentation/profile-page/profile-page.component').then(
            (module) => module.ProfilePageComponent
          )
      },
      {
        path: 'subscriptions',
        loadComponent: () =>
          import('./features/account/presentation/subscriptions-page/subscriptions-page.component').then(
            (module) => module.SubscriptionsPageComponent
          )
      },
      {
        path: 'family-sharing',
        canActivate: [premiumGuard],
        loadComponent: () =>
          import('./features/account/presentation/family-sharing-page/family-sharing-page.component').then(
            (module) => module.FamilySharingPageComponent
          )
      }
    ]
  },
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/presentation/auth-shell/auth-shell.component').then(
        (module) => module.AuthShellComponent
      ),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'login'
      },
      {
        path: 'login',
        loadComponent: () =>
          import('./features/auth/presentation/login-page/login-page.component').then(
            (module) => module.LoginPageComponent
          )
      },
      {
        path: 'register',
        loadComponent: () =>
          import('./features/auth/presentation/register-page/register-page.component').then(
            (module) => module.RegisterPageComponent
          )
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
