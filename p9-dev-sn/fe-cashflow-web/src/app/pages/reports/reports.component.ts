import { Component, OnInit } from '@angular/core';
import { ReportService } from '../../services/report.service';
import { CashStatementReportConsolidated } from '../../models/cash.statement.report.consolidated';
import { Paged } from '../../models/paged.model';
import { ChartConfiguration, ChartType } from 'chart.js';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-reports',
    templateUrl: './reports.component.html',
    standalone: false
})
export class ReportsComponent implements OnInit {
  loading = true;

  reportData: CashStatementReportConsolidated[] = [];
  cashStatementReportConsolidated: Paged<CashStatementReportConsolidated> = {
    totalItems: 0,
    totalPages: 0,
    page: 1,
    pageSize: 10,
    items: []
  };

  page = 1;
  pageSize = 30;
  interval = 'D';
  viewTotalItems = 10;

  public barChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: []
  };
  public barChartOptions: ChartConfiguration<'bar'>['options'] = {
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
  public barChartType: ChartType = 'bar';

  constructor(private readonly reportService: ReportService, private readonly toast: ToastrService) {}

  ngOnInit(): void {
    this.fetchCashStatementReportConsolidated();
  }

  fetchCashStatementReportConsolidated(): void {
    this.loading = true;
    this.reportService.getCashStatementConsolidated(this.interval, this.page, this.pageSize).subscribe({
      next: (data) => {
        if(!data || !data.items) {
          this.toast.warning('Nenhum dado encontrado para o período selecionado.');
          this.loading = false;
          return;
        }

        this.cashStatementReportConsolidated = data;
        this.reportData = data.items;
        this.viewTotalItems = data.items.length;

        // POPULA GRÁFICO ==>
        const groupedByDate = new Map<string, { balance: number; }>();

        this.reportData.forEach(data => {
          const reportDate = new Date(data.date);
          const date = reportDate.toISOString().split('T')[0];

          if (!groupedByDate.has(date)) {
            groupedByDate.set(date, { balance: 0 });
          }
          const current = groupedByDate.get(date)!;
          current.balance += data.totalBalance;

        });

        const sortedDates = Array.from(groupedByDate.keys()).sort();
        const balanceData: number[] = [];
        const graphLabels: string[] = [];

        sortedDates.forEach(_date => {
          const date = new Date(_date);
          const day = date.getDate();
          const month = date.toLocaleString('pt-Br', { month: 'short' });
          if(this.interval === 'D') {
            graphLabels.push(`${day+1}/${month.charAt(0).toUpperCase() + month.slice(1, 3)}`);
          }
          if(this.interval === 'M') {
            graphLabels.push(`${month.charAt(0).toUpperCase() + month.slice(1, 3)}/${date.getFullYear()}`);
          }

          const totals = groupedByDate.get(_date)!;
          balanceData.push(totals.balance);
        });

        this.barChartData = {
          labels: graphLabels,
          datasets: [
            {
              data: balanceData,
              label: 'Saldo Final',
              borderWidth: 0.8,
              borderColor: balanceData.map(value => value >= 0 ? 'blue' : 'red'),
              backgroundColor: balanceData.map(value => value >= 0 ? 'rgba(30, 91, 198, 0.3)' : 'rgba(255, 32, 32, 0.1)'),
            }
          ]
        };
        // <== POPULA GRÁFICO

        this.loading = false;
      },
      error: (error) => {
        console.error('Erro para consultar relatório');
        this.loading = false;
      }
    });
  }
  
  changePeriod(period: string): void {
    this.interval = period;
    this.fetchCashStatementReportConsolidated();
  }

  goToPage(newPage: number) {
    if (newPage >= 1 && newPage <= this.cashStatementReportConsolidated.totalPages) {
      this.page = newPage;
      this.fetchCashStatementReportConsolidated();
    }
  }

  get pageNumbers(): number[] {
    return Array.from({ length: this.cashStatementReportConsolidated.totalPages }, (_, i) => i + 1);
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(amount);
  }

  formatDate(_date: Date): string {
    const date = new Date(_date);

    if(this.interval === 'D') {
      return `${ date.getDate().toString().padStart(2, '0') }/${(date.getMonth()+1).toString().padStart(2, '0') }`;
    }

    const month = date.toLocaleString('pt-Br', { month: 'short' });
    return `${month.charAt(0).toUpperCase() + month.slice(1, 3)}/${date.getFullYear()}`;
  }
}