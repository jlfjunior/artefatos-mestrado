import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then(c => c.LoginComponent)
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(c => c.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'lancamentos',
    loadComponent: () => import('./features/lancamentos/lancamentos.component').then(c => c.LancamentosComponent),
    canActivate: [authGuard]
  },
  {
    path: 'consolidado',
    loadComponent: () => import('./features/consolidado/consolidado.component').then(c => c.ConsolidadoComponent),
    canActivate: [authGuard]
  },
  { path: '**', redirectTo: 'dashboard' }
];
