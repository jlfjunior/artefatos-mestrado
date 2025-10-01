using Commons.Infra.RabbitMQ.Events;
using Commons.Infra.RabbitMQ.Interfaces;
using Transaction.Application.Interfaces.Repository;
using Transaction.Application.UseCases.Transaction.Commons;
using Transaction.Domain.Entity;

namespace Transaction.Application.UseCases.Transaction.Create;
public class CreateMovement : ICreateMovement
{
    private readonly IEventPublisher _publisher;
    private readonly IMovementRepository _repository;
    public CreateMovement(IEventPublisher publisher, IMovementRepository repository)
    {
        _publisher = publisher;
        _repository = repository;
    }

    public async Task<MovementOutput> Handle(CreateMovementInput request, CancellationToken cancellationToken)
    {
        Movement movement = new Movement(request.Description, request.Value, request.Data);

        await _repository.SaveAsync(movement);

        var transactionId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var evt = new CreatedTransactionEvent
        (transactionId, movement.Id, movement.Description, movement.Value, movement.Data, createdAt);

        _publisher.PublishCreatedTransaction(evt);

        return MovementOutput.FromDomain(movement);
    }
}
