namespace ControleLancamentos.Application.Lancamentos.Dtos;

/// <summary>
/// Resultado do consolidado diário para uma data específica.
/// </summary>
public sealed record ConsolidadoDiarioResponse(
    DateOnly DataReferencia,
    decimal TotalCreditos,
    decimal TotalDebitos,
    decimal Saldo,
    int QuantidadeLancamentos);
