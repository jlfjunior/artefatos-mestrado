namespace ControleLancamentos.Application.Lancamentos.Dtos;

/// <summary>
/// Data de referência para o cálculo do consolidado diário.
/// </summary>
public sealed record ConsolidadoDiarioRequest(DateOnly? DataReferencia);
