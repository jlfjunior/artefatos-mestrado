import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { LayoutComponent } from './layouts/layout/layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { TransactionsComponent } from './pages/transactions/transactions.component';
import { ReportsComponent } from './pages/reports/reports.component';

export const routes: Routes = [
  { 
    path: 'login', 
    component: LoginComponent 
  },
  {
    path: '',
    component: LayoutComponent,
    children: [
      { 
        path: 'dashboard', 
        component: DashboardComponent 
      },
      { 
        path: 'transactions',
        component: TransactionsComponent 
      },
      { 
        path: 'reports', 
        component: ReportsComponent 
      },
      {
        path: '', 
        redirectTo: '/dashboard',
        pathMatch: 'full'
      }
    ]
  },
  { 
    path: '**', 
    redirectTo: '/login'
  }
];
