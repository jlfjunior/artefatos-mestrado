using ControleLancamentos.Domain.Lancamentos;

namespace ControleLancamentos.Application.Lancamentos.Dtos;

/// <summary>
/// Dados retornados após a criação de um lançamento.
/// </summary>
public sealed record CriarLancamentoResponse(
    Guid Id,
    decimal Valor,
    TipoLancamento Tipo,
    DateTime DataLancamento,
    string? Descricao);
