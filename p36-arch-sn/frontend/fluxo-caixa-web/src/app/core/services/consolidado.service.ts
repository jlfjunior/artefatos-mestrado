import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ConsolidadoDiario {
  data: string;
  totalCreditos: number;
  totalDebitos: number;
  saldoConsolidado: number;
  quantidadeLancamentos: number;
  ultimaAtualizacao: string;
}

@Injectable({
  providedIn: 'root'
})
export class ConsolidadoService {
  private apiUrl = `${environment.apiUrl}/consolidado`;

  constructor(private http: HttpClient) {}

  getConsolidado(data: string): Observable<ConsolidadoDiario> {
    return this.http.get<ConsolidadoDiario>(`${this.apiUrl}/${data}`);
  }
}
