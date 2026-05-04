import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Lancamento {
  id: string;
  tipo: string;
  valor: number;
  descricao: string;
  dataHora: string;
  criadoEm: string;
  dataHoraLocal: string;
}

export interface CreateLancamentoDto {
  tipo: string;
  valor: number;
  descricao: string;
  dataHora: string;
}

@Injectable({
  providedIn: 'root'
})
export class LancamentosService {
  private apiUrl = `${environment.apiUrl}/lancamentos`;

  constructor(private http: HttpClient) {}

  getLancamentos(data: string, fusoHorario: string = 'America/Sao_Paulo'): Observable<Lancamento[]> {
    let params = new HttpParams().set('data', data).set('fusoHorario', fusoHorario);
    return this.http.get<Lancamento[]>(this.apiUrl, { params });
  }

  criarLancamento(dto: CreateLancamentoDto): Observable<Lancamento> {
    return this.http.post<Lancamento>(this.apiUrl, dto);
  }
}
