import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Transaction } from '../models/transaction.model';
import { TransactionSummary } from '../models/transaction.summary.model';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Paged } from '../models/paged.model';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private readonly apiUrl = environment.cashflowTransactionApi;

  constructor(private http: HttpClient) { }

  getTransactionsSummary(): Observable<TransactionSummary[]> {
    return this.http.get<TransactionSummary[]>(`${this.apiUrl}/v1/transactions/summary`);
  }

  getTransactions(type: string, category: string, search: string, page: number, pageSize: number): Observable<Paged<Transaction>> {
    return this.http.get<Paged<Transaction>>(`${this.apiUrl}/v1/transactions?type=${type}&category=${category}&search=${search}&page=${page}&pageSize=${pageSize}`);
  }

  addTransaction(transaction: Omit<Transaction, 'transactionId'>): Observable<HttpResponse<Transaction>> {
    return this.http.post<Transaction>(`${this.apiUrl}/v1/transactions`, transaction, { observe: 'response' });
  }

  addMassiveTransactions(file: File): Observable<HttpResponse<Transaction>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<Transaction>(`${this.apiUrl}/v1/transactions/massive-create`, formData, { observe: 'response' });
  }

  updateTransaction(transaction: Transaction): Observable<Transaction> {
    let body = {
      description: transaction.description,
      type: transaction.type,
      category: transaction.category,
      amount: transaction.amount,
      date: transaction.date
    };

    return this.http.put<Transaction>(`${this.apiUrl}/v1/transactions/${transaction.transactionId}`, body);
  }

  deleteTransaction(transactionId: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/v1/transactions/${transactionId}`);
  }
}