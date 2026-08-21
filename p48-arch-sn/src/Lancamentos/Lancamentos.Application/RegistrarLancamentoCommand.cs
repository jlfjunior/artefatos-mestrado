using Lancamentos.Domain;

namespace Lancamentos.Application;

/// <summary>Entrada do caso de uso de registrar lançamento.</summary>
public record RegistrarLancamentoCommand(
    TipoLancamento Tipo,
    decimal Valor,
    DateOnly Data,
    string? Descricao);
