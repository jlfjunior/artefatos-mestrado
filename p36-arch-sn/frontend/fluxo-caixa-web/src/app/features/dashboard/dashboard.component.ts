import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FluxoCaixaService, LancamentoDto, ConsolidadoDto, CreateLancamentoCommand } from '../../core/services/fluxo-caixa.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  private fluxoCaixaService = inject(FluxoCaixaService);

  hoje: string = new Date().toISOString().split('T')[0];

  consolidado: ConsolidadoDto | null = null;
  lancamentos: LancamentoDto[] = [];
  
  // Quick form model
  novoLancamento: CreateLancamentoCommand = {
    descricao: '',
    valor: 0,
    tipo: 'Débito',
    dataHora: new Date().toISOString()
  };

  isSubmitting = false;

  ngOnInit(): void {
    this.carregarDados();
  }

  carregarDados() {
    this.fluxoCaixaService.getConsolidado(this.hoje).subscribe({
      next: (dados) => this.consolidado = dados,
      error: (err) => console.error('Erro ao buscar consolidado:', err)
    });

    this.fluxoCaixaService.getLancamentos(this.hoje).subscribe({
      next: (dados) => this.lancamentos = dados,
      error: (err) => console.error('Erro ao buscar lançamentos:', err)
    });
  }

  registrarLancamento() {
    if (!this.novoLancamento.descricao || !this.novoLancamento.valor) return;

    this.isSubmitting = true;
    this.novoLancamento.dataHora = new Date().toISOString();

    this.fluxoCaixaService.createLancamento(this.novoLancamento).subscribe({
      next: () => {
        // Reset the form
        this.novoLancamento = {
          descricao: '',
          valor: 0,
          tipo: 'Débito',
          dataHora: new Date().toISOString()
        };
        this.isSubmitting = false;

        // Reload the lists
        // Note: Event-driven architecture takes a bit to update consolidado (MassTransit + RabbitMQ)
        // We reload instantly to get exactly what's there, but maybe wait 500ms
        setTimeout(() => this.carregarDados(), 500);
      },
      error: (err) => {
        console.error('Erro ao criar lançamento:', err);
        this.isSubmitting = false;
        alert('Erro ao registrar lançamento: ' + (err.error?.Errors ? err.error.Errors.join(', ') : err.message));
      }
    });
  }
}
