using Domain.Entities.Launch;
using Infrastructure.Persistence.Data;
using Repositories.Repositories.Generic.GenericSQL;

namespace Domain.Entities.LaunchProduct;

public class LaunchProductRepository : Repository<LaunchProductEntity>, ILaunchProductRepository
{
    public LaunchProductRepository(ApplicationDbContext context) : base(context)
    {
    }
}