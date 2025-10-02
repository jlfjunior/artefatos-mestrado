using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces;
using FluxoCaixa.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluxoCaixa.Infrastructure.Repositories
{
    public class LancamentoRepository : ILancamentoRepository
    {
        private readonly AppDbContext _context;

        public LancamentoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Lancamento lancamento)
        {
            await _context.Lancamentos.AddAsync(lancamento);
        }

        public async Task<Lancamento?> ObterPorIdAsync(Guid id)
        {
            return await _context.Lancamentos.FindAsync(id);
        }

        public async Task<List<Lancamento>> ObterTodosAsync()
        {
            return await _context.Lancamentos.ToListAsync();
        }

        public async Task AtualizarAsync(Lancamento lancamento)
        {
            _context.Lancamentos.Update(lancamento);
            await Task.CompletedTask;
        }

        public async Task ExcluirAsync(Lancamento lancamento)
        {
            _context.Lancamentos.Remove(lancamento);
            await Task.CompletedTask;
        }
    }
}
