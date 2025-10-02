import { Component, OnInit } from '@angular/core';
import { TransactionService } from '../../services/transaction.service';
import { ChartConfiguration, ChartType } from 'chart.js';
import { ManagementService } from '../../services/management.service';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-dashboard',
    templateUrl: './dashboard.component.html',
    standalone: false
})
export class DashboardComponent implements OnInit {
  loading = true;
  
  balance = 0;
  income = 0;
  expenses = 0;
  
  recentTransactions: any[] = [];

  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: []
  };
  public lineChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        grid: {
          display: false
        }
      },
      y: {
        grid: {
          display: false
        }
      }
    }
  };

  public lineChartType: ChartType = 'line';

  constructor(
    private readonly transactionService: TransactionService, 
    private readonly managementService: ManagementService, 
    private readonly toast: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadBalanceData();
    this.loadTransactionsData();
  }

  loadTransactionsData(): void {
    this.loading = true;

    this.transactionService.getTransactionsSummary().subscribe({
      next: (transactions) => {
        if (!transactions || transactions.length === 0) {
          this.toast.warning('Nenhuma transação encontrada.');
          this.loading = false;
          return;
        }

        this.recentTransactions = transactions.slice(0, 5);

        // POPULA GRÁFICO ==>
        const groupedByDate = new Map<string, { income: number; expense: number }>();

        transactions.forEach(trans => {
          const transDate = new Date(trans.date);
          const date = transDate.toISOString().split('T')[0];

          if (!groupedByDate.has(date)) {
            groupedByDate.set(date, { income: 0, expense: 0 });
          }

          const current = groupedByDate.get(date)!;

          if (trans.type === 'Income') {
            current.income += trans.amount;
          } else if (trans.type === 'Expense') {
            current.expense += trans.amount;
          }

        });

        const sortedDates = Array.from(groupedByDate.keys()).sort();
        const incomeData: number[] = [];
        const expenseData: number[] = [];
        const graphLabels: string[] = [];

        sortedDates.forEach(_date => {
          const date = new Date(_date);
          const day = date.getDate();
          const month = date.toLocaleString('pt-Br', { month: 'short' });
          graphLabels.push(`${day+1}/${month.charAt(0).toUpperCase() + month.slice(1)}`);

          const totals = groupedByDate.get(_date)!;
          incomeData.push(totals.income);
          expenseData.push(totals.expense);
        });

        this.lineChartData = {
          labels: graphLabels,
          datasets: [
            {
              data: incomeData,
              label: 'Entradas',
              fill: true,
              tension: 0.3,
              borderWidth: 0.4,
              borderColor: 'blue',
              backgroundColor: 'rgba(30, 91, 198, 0.3)',
              pointBackgroundColor: 'rgba(30, 91, 198, 0.5)'
            },
            {
              data: expenseData,
              label: 'Saídas',
              fill: true,
              tension: 0.5,
              borderWidth: 0.4,
              borderColor: 'red',
              backgroundColor: 'rgba(255, 32, 32, 0.1)',
              pointBackgroundColor: 'rgba(255, 32, 32, 0.5)'
            }
          ]
        };
        // <== POPULA GRÁFICO

        this.loading = false;
      },
      error: () => {
        this.toast.error('Erro ao carregar transações recentes.');
        this.loading = false;
      }
    });
  }

  loadBalanceData(): void {
    this.loading = true;
    
    this.managementService.getBalanceSummary().subscribe(balance => {
      this.balance = balance.currentBalance;
      this.income = balance.totalInflows;
      this.expenses = balance.totalOutflows;
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('pt-Br', {
      style: 'currency',
      currency: 'BRL'
    }).format(amount);
  }
}