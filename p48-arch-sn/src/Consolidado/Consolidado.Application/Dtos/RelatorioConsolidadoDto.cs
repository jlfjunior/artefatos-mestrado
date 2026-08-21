namespace Consolidado.Application.Dtos;

/// <summary>
/// Relatório do saldo diário consolidado num período: a lista de dias com
/// movimento e os totais agregados do intervalo.
/// </summary>
public record RelatorioConsolidadoDto(
    DateOnly De,
    DateOnly Ate,
    IReadOnlyList<SaldoDiarioDto> Dias,
    decimal TotalCreditos,
    decimal TotalDebitos,
    decimal Saldo);
