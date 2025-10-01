using Transaction.Domain.Entity;

namespace Transaction.Application.Interfaces.Repository;

public interface IMovementRepository
{
    Task SaveAsync(Movement movement);

}
