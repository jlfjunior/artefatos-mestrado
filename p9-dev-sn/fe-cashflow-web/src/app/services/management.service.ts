import { HttpClient } from "@angular/common/http";
import { environment } from "../../environments/environment";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { BalanceSummary } from "../models/balance.summary.model";

@Injectable({
  providedIn: 'root'
})

export class ManagementService {
  private readonly apiUrl = environment.cashflowManagementApi;

  constructor(private http: HttpClient) { }

  getBalanceSummary(): Observable<BalanceSummary> {
    return this.http.get<BalanceSummary>(`${this.apiUrl}/v1/cash-statement/summary`);
  }
}