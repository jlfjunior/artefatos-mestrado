import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FluxoCaixaService, ConsolidadoDto } from '../../core/services/fluxo-caixa.service';

export interface ConsolidadoAgregado {
  label: string;
  totalCreditos: number;
  totalDebitos: number;
  saldoConsolidado: number;
  quantidadeLancamentos: number;
  ultimaAtualizacao: string;
  dias: ConsolidadoDto[];
}

@Component({
  selector: 'app-consolidado',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './consolidado.component.html',
  styleUrls: ['./consolidado.component.scss']
})
export class ConsolidadoComponent implements OnInit {
  private svc = inject(FluxoCaixaService);

  hoje = new Date().toISOString().split('T')[0];
  dataInicio = this.hoje;
  dataFim = this.hoje;
  periodo: 'hoje' | '30d' | '90d' | 'custom' = 'hoje';

  consolidadoUnico: ConsolidadoDto | null = null;
  agregado: ConsolidadoAgregado | null = null;
  isLoading = false;
  erro = '';

  get modoRange(): boolean {
    return this.periodo === '30d' || this.periodo === '90d';
  }

  ngOnInit() { this.carregar(); }

  setPeriodo(p: 'hoje' | '30d' | '90d' | 'custom') {
    this.periodo = p;
    const hoje = new Date();

    if (p === 'hoje') {
      this.dataInicio = this.hoje;
      this.dataFim = this.hoje;
    } else if (p === '30d') {
      const d = new Date(hoje);
      d.setDate(d.getDate() - 30);
      this.dataInicio = d.toISOString().split('T')[0];
      this.dataFim = this.hoje;
    } else if (p === '90d') {
      const d = new Date(hoje);
      d.setDate(d.getDate() - 90);
      this.dataInicio = d.toISOString().split('T')[0];
      this.dataFim = this.hoje;
    }

    if (p !== 'custom') this.carregar();
  }

  carregar() {
    this.isLoading = true;
    this.erro = '';
    this.consolidadoUnico = null;
    this.agregado = null;

    if (this.modoRange) {
      this.svc.getConsolidadoRange(this.dataInicio, this.dataFim).subscribe({
        next: dias => {
          const totalCreditos = dias.reduce((s, d) => s + d.totalCreditos, 0);
          const totalDebitos = dias.reduce((s, d) => s + d.totalDebitos, 0);
          const qtd = dias.reduce((s, d) => s + d.quantidadeLancamentos, 0);
          const ultima = dias.length ? dias[dias.length - 1].ultimaAtualizacao : new Date().toISOString();
          const label = this.periodo === '30d' ? 'Últimos 30 Dias' : 'Últimos 90 Dias';

          this.agregado = {
            label,
            totalCreditos,
            totalDebitos,
            saldoConsolidado: totalCreditos - totalDebitos,
            quantidadeLancamentos: qtd,
            ultimaAtualizacao: ultima,
            dias: [...dias].reverse()
          };
          this.isLoading = false;
        },
        error: () => { this.erro = 'Não foi possível carregar o consolidado do período.'; this.isLoading = false; }
      });
    } else {
      const data = this.periodo === 'custom' ? this.dataInicio : this.hoje;
      this.svc.getConsolidado(data).subscribe({
        next: d => { this.consolidadoUnico = d; this.isLoading = false; },
        error: () => { this.erro = 'Não foi possível carregar o consolidado.'; this.isLoading = false; }
      });
    }
  }
}
