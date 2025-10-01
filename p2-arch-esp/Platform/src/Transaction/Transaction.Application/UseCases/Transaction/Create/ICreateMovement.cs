using MediatR;
using Transaction.Application.UseCases.Transaction.Commons;

namespace Transaction.Application.UseCases.Transaction.Create;

public interface ICreateMovement : IRequestHandler<CreateMovementInput, MovementOutput>
{
}
