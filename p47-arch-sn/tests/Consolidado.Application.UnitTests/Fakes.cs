using Consolidado.Application.Dtos;
using Consolidado.Application.Ports;
using Consolidado.Domain;

namespace Consolidado.Application.UnitTests;

/// <summary>
/// Dublês em memória das portas do read side. Mantenho-os num arquivo só para os
/// testes de unidade dos dois serviços de aplicação compartilharem sem ruído.
/// </summary>
internal sealed class SaldoRepositorioFake : ISaldoDiarioRepository
{
    private readonly Dictionary<DateOnly, SaldoDiario> _saldos = new();

    public int Salvamentos { get; private set; }

    public void Semear(SaldoDiario saldo) => _saldos[saldo.Data] = saldo;

    public Task<SaldoDiario?> ObterPorDataAsync(DateOnly data, CancellationToken ct = default)
        => Task.FromResult(_saldos.GetValueOrDefault(data));

    public Task<IReadOnlyList<SaldoDiario>> ListarPorPeriodoAsync(DateOnly de, DateOnly ate, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<SaldoDiario>>(
            _saldos.Values.Where(s => s.Data >= de && s.Data <= ate).OrderBy(s => s.Data).ToList());

    public Task SalvarAsync(SaldoDiario saldo, CancellationToken ct = default)
    {
        _saldos[saldo.Data] = saldo;
        Salvamentos++;
        return Task.CompletedTask;
    }
}

internal sealed class ProcessadosStoreFake : ILancamentosProcessadosStore
{
    private readonly HashSet<Guid> _processados = new();

    public void MarcarPreexistente(Guid id) => _processados.Add(id);

    public Task<bool> JaProcessadoAsync(Guid lancamentoId, CancellationToken ct = default)
        => Task.FromResult(_processados.Contains(lancamentoId));

    public Task MarcarComoProcessadoAsync(Guid lancamentoId, CancellationToken ct = default)
    {
        _processados.Add(lancamentoId);
        return Task.CompletedTask;
    }
}

internal sealed class SaldoCacheFake : ISaldoCache
{
    private readonly Dictionary<DateOnly, SaldoDiarioDto> _cache = new();

    public int Invalidacoes { get; private set; }
    public int Gravacoes { get; private set; }
    public int Leituras { get; private set; }

    public void Semear(SaldoDiarioDto dto) => _cache[dto.Data] = dto;

    public Task<SaldoDiarioDto?> ObterAsync(DateOnly data, CancellationToken ct = default)
    {
        Leituras++;
        return Task.FromResult(_cache.GetValueOrDefault(data));
    }

    public Task GravarAsync(SaldoDiarioDto saldo, CancellationToken ct = default)
    {
        _cache[saldo.Data] = saldo;
        Gravacoes++;
        return Task.CompletedTask;
    }

    public Task InvalidarAsync(DateOnly data, CancellationToken ct = default)
    {
        _cache.Remove(data);
        Invalidacoes++;
        return Task.CompletedTask;
    }
}

internal sealed class UnitOfWorkFake : IUnitOfWork
{
    public int Commits { get; private set; }

    public Task CommitAsync(CancellationToken ct = default)
    {
        Commits++;
        return Task.CompletedTask;
    }
}
