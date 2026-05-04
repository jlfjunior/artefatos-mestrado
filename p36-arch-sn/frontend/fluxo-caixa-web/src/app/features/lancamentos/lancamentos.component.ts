import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FluxoCaixaService, LancamentoDto, CreateLancamentoCommand } from '../../core/services/fluxo-caixa.service';

@Component({
  selector: 'app-lancamentos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lancamentos.component.html',
  styleUrls: ['./lancamentos.component.scss']
})
export class LancamentosComponent implements OnInit {
  private svc = inject(FluxoCaixaService);

  hoje = new Date().toISOString().split('T')[0];
  dataSelecionada = this.hoje;
  lancamentos: LancamentoDto[] = [];
  isLoading = false;
  isSubmitting = false;
  showForm = false;

  novoLancamento: CreateLancamentoCommand = {
    descricao: '',
    valor: 0,
    tipo: 'Débito',
    dataHora: new Date().toISOString()
  };

  ngOnInit() { this.carregar(); }

  carregar() {
    this.isLoading = true;
    this.svc.getLancamentos(this.dataSelecionada).subscribe({
      next: d => { this.lancamentos = d; this.isLoading = false; },
      error: () => this.isLoading = false
    });
  }

  registrar() {
    if (!this.novoLancamento.descricao || this.novoLancamento.valor <= 0) return;
    this.isSubmitting = true;
    this.novoLancamento.dataHora = new Date().toISOString();
    this.svc.createLancamento(this.novoLancamento).subscribe({
      next: () => {
        this.novoLancamento = { descricao: '', valor: 0, tipo: 'Débito', dataHora: new Date().toISOString() };
        this.isSubmitting = false;
        this.showForm = false;
        setTimeout(() => this.carregar(), 500);
      },
      error: err => {
        this.isSubmitting = false;
        alert('Erro: ' + (err.error?.Errors?.join(', ') || err.message));
      }
    });
  }
}
