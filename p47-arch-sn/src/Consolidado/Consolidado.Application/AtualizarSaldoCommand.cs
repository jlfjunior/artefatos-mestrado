namespace Consolidado.Application;

/// <summary>
/// Dados de um lançamento já desacoplados do contrato de mensageria. O consumer
/// traduz o evento de integração para este comando antes de chamar o caso de uso.
/// </summary>
public record AtualizarSaldoCommand(
    Guid LancamentoId,
    string Tipo,
    decimal Valor,
    DateOnly Data);
