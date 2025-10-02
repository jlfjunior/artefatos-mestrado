import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TransactionService } from '../../services/transaction.service';
import { Transaction } from '../../models/transaction.model';
import { Paged } from '../../models/paged.model';
import { HttpResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-transactions',
    templateUrl: './transactions.component.html',
    standalone: false
})
export class TransactionsComponent implements OnInit {
  transactions: Paged<Transaction> = {
    totalItems: 0,
    totalPages: 0,
    page: 1,
    pageSize: 10,
    items: []
  };

  page = 1;
  pageSize = 10;
  viewTotalItems = 10;

  filteredTransactions: Transaction[] = [];
  loading = true;
  showModal = false;
  transactionForm: FormGroup;
  isEditing = false;
  currentTransactionId: string | null = null;
  
  filterForm: FormGroup;
  transactionTypes = [
    { value: '', label: 'Todos' },
    { value: 'Income', label: 'Entrada' },
    { value: 'Expense', label: 'Saída' }
  ]
  categories = ['Todas', 'Vendas', 'Lucros', 'Pagamentos', 'Folha de Pagamento'];
  
  constructor(
    private readonly transactionService: TransactionService, 
    private readonly formBuilder: FormBuilder, 
    private readonly toast: ToastrService
  ) {
    this.transactionForm = this.formBuilder.group({
      date: [new Date(), Validators.required],
      description: ['', Validators.required],
      amount: [0, Validators.required],
      type: ['Entrada', Validators.required],
      category: ['', Validators.required],
    });
    
    this.filterForm = this.formBuilder.group({
      type: [''],
      category: ['Todas'],
      search: ['']
    });
  }

  ngOnInit(): void {
    this.loadTransactions();

    this.filterForm.valueChanges.subscribe(() => {
      this.applyFilters();
    });
  }

  loadTransactions(): void {
    this.fetchTransactions('', '', '', this.page, this.pageSize);
  }

  fetchTransactions(type: string, category: string, search: string, page: number, pageSize: number): void {
    this.loading = true;
    if(category === 'Todas') {
      category = '';
    }

    this.transactionService.getTransactions(type, category, search, page, pageSize).subscribe({
      next: (response: Paged<Transaction>) => {
        if(!response || !response.items) {
          this.toast.warning('Nenhuma transação encontrada.');  
          this.loading = false;
          return;
        }

        this.transactions = response;
        this.viewTotalItems = response.items.length;
        this.filteredTransactions = response.items;
        this.loading = false;
      },
      error: (error) => {
        this.toast.error('Erro ao carregar transações: ' + error.message);
        this.loading = false;
      }
    });
  }
  
  applyFilters(): void {
    const filters = this.filterForm.value;
    this.fetchTransactions(filters.type, filters.category, filters.search, this.page, this.pageSize);
  }

  goToPage(newPage: number) {
    if (newPage >= 1 && newPage <= this.transactions.totalPages) {
      this.page = newPage;
      this.fetchTransactions(this.filterForm.value.type, this.filterForm.value.category, this.filterForm.value.search, newPage, this.pageSize);
    }
  }

  get pageNumbers(): number[] {
    return Array.from({ length: this.transactions.totalPages }, (_, i) => i + 1);
  }

  openAddModal(): void {
    this.isEditing = false;
    this.transactionForm.reset({
      date: new Date(),
      type: 'Income'
    });
    this.showModal = true;
  }
  
  openEditModal(transaction: Transaction): void {
    this.isEditing = true;
    this.currentTransactionId = transaction.transactionId;
    
    const amount = transaction.type === 'saída' ? Math.abs(transaction.amount) : transaction.amount;
    
    this.transactionForm.setValue({date: new Date(transaction.date).toISOString().substring(0,10), description: transaction.description, amount: amount, type: transaction.type, category: transaction.category });
    
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
  }

  createTransaction(): void {
    if (this.transactionForm.invalid) {
      return;
    }

    let { date, description, type, amount, category } = this.transactionForm.value;

    const newTransaction = { description, type, category, amount, date };

    this.transactionService.addTransaction(newTransaction).subscribe({
      next: () => {
        this.toast.success('Transação adicionada com sucesso!');
        this.loadTransactions();
        this.closeModal();
      },
      error: () => {
        this.toast.error('Erro ao adicionar transação');
      }
    });
  }

  editTransaction(): void {
    if (this.transactionForm.invalid || !this.currentTransactionId) {
      return;
    }

    const { date, description, type, amount, category } = this.transactionForm.value;

    const updatedTransaction: Transaction = {
      transactionId: this.currentTransactionId,
      description,
      type,
      category,
      amount,
      date
    };

    this.transactionService.updateTransaction(updatedTransaction).subscribe({
      next: () => {
        this.toast.success('Transação atualizada com sucesso!');
        this.loadTransactions();
        this.closeModal();
      },
      error: () => {
        this.toast.error('Erro ao atualizar transação');
      }
    });
  }
  
  deleteTransaction(id: string): void {
    if (confirm('Deseja excluir essa transação?')) {
      this.transactionService.deleteTransaction(id).subscribe({
        next: () => {
          this.toast.success('Transação excluída com sucesso!');
          this.loadTransactions();
        },
        error: () => {
          this.toast.error('Erro ao excluir transação');
        }
      });
    }
  }
  
  importTransactions(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) {
      return;
    }

    this.transactionService.addMassiveTransactions(input.files[0]).subscribe({
      next: (response: HttpResponse<Transaction>) => {
        this.toast.success('Transações importadas com sucesso!');
        this.loadTransactions();
      }, 
      error: () => {
        this.toast.error('Erro ao importar transações');
      }
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('pt-Br', {
      style: 'currency',
      currency: 'BRL'
    }).format(amount);
  }
}