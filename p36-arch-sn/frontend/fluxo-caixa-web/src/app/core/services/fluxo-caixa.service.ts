import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LancamentoDto {
  id: string;
  tipo: string;
  valor: number;
  descricao: string;
  dataHora: string;
  dataHoraLocal: string;
  criadoEm: string;
}

export interface ConsolidadoDto {
  data: string;
  totalCreditos: number;
  totalDebitos: number;
  saldoConsolidado: number;
  quantidadeLancamentos: number;
  ultimaAtualizacao: string;
}

export interface CreateLancamentoCommand {
  tipo: string;
  valor: number;
  descricao: string;
  dataHora: string;
}

// A API do Lancamentos retorna um objeto aninhado em alguns cenários
interface LancamentosResponse {
  value?: LancamentoDto[];
}

@Injectable({
  providedIn: 'root'
})
export class FluxoCaixaService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getLancamentos(data: string, fusoHorario = 'America/Sao_Paulo'): Observable<LancamentoDto[]> {
    return this.http.get<LancamentoDto[] | LancamentosResponse>(
      `${this.baseUrl}/lancamentos?data=${data}&fusoHorario=${fusoHorario}`
    ).pipe(
      // Normaliza a resposta — o ASP.NET retorna { value: [...] } em alguns casos
      map(res => Array.isArray(res) ? res : (res as LancamentosResponse).value ?? [])
    );
  }

  getConsolidado(data: string): Observable<ConsolidadoDto> {
    return this.http.get<ConsolidadoDto>(`${this.baseUrl}/consolidado/${data}`);
  }

  getConsolidadoRange(inicio: string, fim: string): Observable<ConsolidadoDto[]> {
    return this.http.get<ConsolidadoDto[]>(
      `${this.baseUrl}/consolidado/range?inicio=${inicio}&fim=${fim}`
    );
  }

  createLancamento(command: CreateLancamentoCommand): Observable<LancamentoDto> {
    // Normaliza o tipo para o formato esperado pela API (CREDITO / DEBITO)
    const payload = {
      ...command,
      tipo: command.tipo.toUpperCase()
        .normalize('NFD').replace(/[\u0300-\u036f]/g, '') // remove acentos: Débito -> DEBITO
    };
    return this.http.post<LancamentoDto>(`${this.baseUrl}/lancamentos`, payload);
  }
}
