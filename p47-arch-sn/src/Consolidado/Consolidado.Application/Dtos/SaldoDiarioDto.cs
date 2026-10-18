namespace Consolidado.Application.Dtos;

public record SaldoDiarioDto(
    DateOnly Data,
    decimal TotalCreditos,
    decimal TotalDebitos,
    decimal Saldo,
    DateTime AtualizadoEmUtc);
