using Transaction.Application.Interfaces.Repository;
using Transaction.Domain.Entity;

namespace Transaction.Infrastructure.Data
{
    public class MovementRepository : IMovementRepository
    {
        public Task SaveAsync(Movement movement)
        {
            return Task.CompletedTask;
        }
    }
}
