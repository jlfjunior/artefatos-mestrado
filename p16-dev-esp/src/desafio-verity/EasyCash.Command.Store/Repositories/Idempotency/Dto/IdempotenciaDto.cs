using EasyCash.Abstractions.Idempotency.Entity;

namespace EasyCash.Command.Store.Repositories.Idempotency.Dto;

internal sealed class IdempotenciaDto
{
    public string IdempotencyKey { get; set; }
    public string Request { get; set; }
    public string Response { get; set; }

    public static IdempotencyEntity MapToEntity(IdempotenciaDto dto) =>
        IdempotencyEntity.Create(dto.IdempotencyKey, dto.Request, dto.Response);
}