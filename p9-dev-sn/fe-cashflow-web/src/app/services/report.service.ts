import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { CashStatementReportConsolidated } from '../models/cash.statement.report.consolidated';
import { Paged } from '../models/paged.model';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly apiUrl = environment.cashflowManagementApi;

  constructor(private http: HttpClient) { }

  getCashStatementConsolidated(interval: string, page: number, pageSize: number): Observable<Paged<CashStatementReportConsolidated>> {
    return this.http.get<Paged<CashStatementReportConsolidated>>(`${this.apiUrl}/v1/cash-statement/report/consolidated?interval=${interval}&page=${page}&pageSize=${pageSize}`);
  }

  getCategoryBreakdown(): Observable<{ category: string; amount: number; percentage: number }[]> {
    const data = [
      { category: 'Sales', amount: 58000, percentage: 62 },
      { category: 'Services', amount: 25000, percentage: 27 },
      { category: 'Investments', amount: 8500, percentage: 9 },
      { category: 'Other', amount: 2000, percentage: 2 }
    ];
    
    return of(data).pipe(delay(800));
  }

  getExpenseBreakdown(): Observable<{ category: string; amount: number; percentage: number }[]> {
    const data = [
      { category: 'Operations', amount: 12000, percentage: 30 },
      { category: 'Payroll', amount: 18000, percentage: 45 },
      { category: 'Software', amount: 5000, percentage: 12.5 },
      { category: 'Marketing', amount: 3000, percentage: 7.5 },
      { category: 'Other', amount: 2000, percentage: 5 }
    ];
    
    return of(data).pipe(delay(800));
  }

  getMonthlyCashflow(): Observable<{ month: string; income: number; expenses: number; balance: number }[]> {
    const data = [
      { month: 'Jan', income: 20300, expenses: 15200, balance: 5100 },
      { month: 'Feb', income: 25400, expenses: 16800, balance: 8600 },
      { month: 'Mar', income: 22800, expenses: 14500, balance: 8300 },
      { month: 'Apr', income: 30200, expenses: 18600, balance: 11600 },
      { month: 'May', income: 28700, expenses: 17300, balance: 11400 },
      { month: 'Jun', income: 32500, expenses: 19800, balance: 12700 }
    ];
    
    return of(data).pipe(delay(800));
  }
}