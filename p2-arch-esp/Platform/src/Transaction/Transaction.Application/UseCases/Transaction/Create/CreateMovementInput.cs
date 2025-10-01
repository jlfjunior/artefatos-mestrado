using MediatR;
using Transaction.Application.UseCases.Transaction.Commons;

namespace Transaction.Application.UseCases.Transaction.Create;

public class CreateMovementInput : IRequest<MovementOutput>
{
    public string Description { get; set; } = default!;
    public decimal Value { get; set; }
    public DateTime Data { get; set; }
}
